using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class MyRewardsController : Controller
    {
        private readonly RewardService _rewardService;
        private readonly EmailSender _emailSender;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<MyRewardsController> _logger;

        public MyRewardsController(RewardService rewardService, EmailSender emailSender,
            IAuthorizationService authorizationService, IMapper mapper, ILogger<MyRewardsController> logger)
        {
            _rewardService = rewardService;
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

        [HttpGet]
        public IActionResult Create()
        {
            return View(new RewardInputModel());
        }

        [HttpPost]
        public IActionResult Create(RewardInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);
            if (input.EventIds == null || input.EventIds.Count == 0)
            {
                ModelState.AddModelError("EventIds", "Must have at least one qualifying event");
                return View(input);
            }

            var reward = _mapper.Map<Reward>(input);
            reward.RewardEvents = input.EventIds.Select(eventId => new RewardEvent
            {
                EventId = eventId
            }).ToList();

            reward.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _rewardService.AddReward(reward);
            _rewardService.SaveChanges();

            _logger.LogInformation("{user} created reward {reward}", User.Identity.Name, reward.Id);

            return RedirectToAction("Index");
        }
    }
}
