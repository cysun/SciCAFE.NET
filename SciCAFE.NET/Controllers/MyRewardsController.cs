using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class MyRewardsController : Controller
    {
        private readonly RewardService _rewardService;
        private readonly FileService _fileService;
        private readonly EmailSender _emailSender;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<MyRewardsController> _logger;

        public MyRewardsController(RewardService rewardService, FileService fileService, EmailSender emailSender,
            IAuthorizationService authorizationService, IMapper mapper, ILogger<MyRewardsController> logger)
        {
            _rewardService = rewardService;
            _fileService = fileService;
            _emailSender = emailSender;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_rewardService.GetRewardsByCreator(userId));
        }

        public IActionResult View(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            if (reward.CreatorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(reward);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new RewardInputModel());
        }

        [HttpPost]
        public IActionResult Create(RewardInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);

            var reward = _mapper.Map<Reward>(input);

            reward.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _rewardService.AddReward(reward);
            _rewardService.SaveChanges();

            _logger.LogInformation("{user} created reward {reward}", User.Identity.Name, reward.Id);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("Attachments", new { id = reward.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            return View(_mapper.Map<RewardInputModel>(reward));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(int id, RewardInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);

            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            _mapper.Map(input, reward);
            _rewardService.SaveChanges();

            _logger.LogInformation("{user} edited reward {reward}", User.Identity.Name, reward.Id);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("Attachments", new { id = reward.Id });
        }

        [HttpGet]
        public async Task<IActionResult> AttachmentsAsync(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            return View(reward);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachmentsAsync(int id, IFormFile[] uploadedFiles)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            if (reward.RewardAttachments.Count + uploadedFiles.Length > 3)
                return BadRequest();

            foreach (var uploadedFile in uploadedFiles)
            {
                var file = _fileService.UploadFile(uploadedFile);
                reward.RewardAttachments.Add(new RewardAttachment
                {
                    Reward = reward,
                    File = file
                });
                _logger.LogInformation("{user} uploaded file {file} to reward {reward}",
                    User.Identity.Name, file.Id, reward.Id);
            }

            _rewardService.SaveChanges();

            return Ok();
        }

        public async Task<IActionResult> DeleteAttachmentAsync(int id)
        {
            var attachment = _rewardService.GetAttachment(id);
            if (attachment == null) return NotFound();

            var reward = _rewardService.GetReward(attachment.RewardId);
            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            reward.RewardAttachments.RemoveAll(a => a.Id == id);
            _rewardService.SaveChanges();
            _logger.LogInformation("{user} removed file {file} from reward {reward}",
                User.Identity.Name, attachment.FileId, reward.Id);

            if (!_rewardService.IsAttachedToReward(attachment.FileId))
            {
                _fileService.DeleteFile(attachment.FileId);
                _logger.LogInformation("{user} deleted file {file}", User.Identity.Name, attachment.FileId);
            }

            return RedirectToAction("Attachments", new { id = reward.Id });
        }

        [HttpGet]
        public IActionResult Events(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            return View(reward);
        }

        [HttpGet]
        public IActionResult Summary(int id)
        {
            return View(_rewardService.GetReward(id));
        }

        public IActionResult Submit(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            if (reward.SubmitDate == null)
            {
                reward.SubmitDate = DateTime.Now;
                _rewardService.SaveChanges();
                _logger.LogInformation("{user} submitted reward {reward}", User.Identity.Name, id);
            }
            var msg = _emailSender.CreateReviewRewardMessage(reward);
            if (msg != null) _ = _emailSender.SendAsync(msg);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PublishAsync(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanReviewReward);
            if (!authResult.Succeeded)
                return Submit(id);

            if (reward.SubmitDate == null)
            {
                reward.SubmitDate = DateTime.Now;
                reward.Review = new Review
                {
                    IsApproved = true,
                    Timestamp = DateTime.Now,
                    ReviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                _rewardService.SaveChanges();
                _logger.LogInformation("{user} published reward {reward}", User.Identity.Name, id);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> AddEventsAsync(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanAddQualifyingEvent);
            if (!authResult.Succeeded)
                return Forbid();

            return View(reward);
        }

        [HttpPost]
        public async Task<IActionResult> AddEventsAsync(int id, List<int> eventIds)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanAddQualifyingEvent);
            if (!authResult.Succeeded)
                return Forbid();

            var currentEventIds = reward.RewardEvents.Select(r => r.EventId).ToHashSet();
            var newEventIds = eventIds.Except(currentEventIds).ToList();
            if (newEventIds.Count > 0)
            {
                reward.RewardEvents.AddRange(newEventIds.Select(eventId => new RewardEvent
                {
                    RewardId = id,
                    EventId = eventId
                }));
                _rewardService.SaveChanges();
                _logger.LogInformation("{user} added {events} to reward {reward}", User.Identity.Name, newEventIds, id);
            }

            return RedirectToAction("AddEvents");
        }

        [HttpPut("MyRewards/{rewardId}/NumOfEventsToQualify/{n}")]
        public async Task<IActionResult> SetNumOfEvents(int rewardId, int n)
        {
            var reward = _rewardService.GetReward(rewardId);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            reward.NumOfEventsToQualify = n >= 1 ? n : 1;
            _rewardService.SaveChanges();
            _logger.LogInformation("{user} set num of events to {numOfEvents} for reward {reward}",
                 User.Identity.Name, n, rewardId);

            return Ok();
        }


        [HttpPost("MyRewards/{rewardId}/Events/{eventId}")]
        public async Task<IActionResult> AddEventAsync(int rewardId, int eventId)
        {
            var reward = _rewardService.GetReward(rewardId);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanAddQualifyingEvent);
            if (!authResult.Succeeded)
                return Forbid();

            var rewardEvent = _rewardService.GetRewardEvent(rewardId, eventId);
            if (rewardEvent != null)
                return BadRequest();

            _rewardService.AddRewardEvent(new RewardEvent
            {
                RewardId = rewardId,
                EventId = eventId
            });
            _rewardService.SaveChanges();
            _logger.LogInformation("{user} added event {event} to reward {reward}",
                User.Identity.Name, eventId, rewardId);

            return Ok();
        }

        [HttpDelete("MyRewards/{rewardId}/Events/{eventId}")]
        public async Task<IActionResult> RemoveEventAsync(int rewardId, int eventId)
        {
            var reward = _rewardService.GetReward(rewardId);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEditReward);
            if (!authResult.Succeeded)
                return Forbid();

            var rewardEvent = _rewardService.GetRewardEvent(rewardId, eventId);
            if (rewardEvent == null)
                return BadRequest();

            _rewardService.RemoveRewardEvent(rewardEvent);
            _rewardService.SaveChanges();
            _logger.LogInformation("{user} removed event {event} from reward {reward}",
                User.Identity.Name, eventId, rewardId);

            return Ok();
        }
    }
}
