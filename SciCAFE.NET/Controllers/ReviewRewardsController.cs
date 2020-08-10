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
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize(Policy = Policy.IsRewardReviewer)]
    public class ReviewRewardsController : Controller
    {
        private readonly RewardService _rewardService;
        private readonly EmailSender _emailSender;

        private readonly ILogger<ReviewRewardsController> _logger;

        public ReviewRewardsController(RewardService rewardService, EmailSender emailSender,
            IMapper mapper, ILogger<ReviewRewardsController> logger)
        {
            _rewardService = rewardService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_rewardService.GetUnreviewedRewards());
        }

        public IActionResult Review(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            if (reward.Review?.IsApproved != null)
                return View("Status", new StatusViewModel
                {
                    Message = "This reward has already been reviewed."
                });

            return View(reward);
        }

        private IActionResult Decision(int id, bool IsApproved, string comments)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            if (reward.Review?.IsApproved != null)
                return View("Status", new StatusViewModel
                {
                    Message = "This reward has already been reviewed."
                });

            reward.Review = new Review
            {
                IsApproved = IsApproved,
                Comments = comments,
                Timestamp = DateTime.Now,
                ReviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _rewardService.SaveChanges();

            _logger.LogInformation("{user} reviewed reward {reward} with approval={decision}",
                User.Identity.Name, reward.Id, IsApproved);

            var msg = _emailSender.CreateRewardReviewedMessage(reward);
            if (msg != null) _ = _emailSender.SendAsync(msg);

            return RedirectToAction("Index");
        }

        public IActionResult Approve(int id, string comments)
        {
            return Decision(id, true, comments);
        }

        public IActionResult Deny(int id, string comments)
        {
            if (string.IsNullOrEmpty(comments))
                return RedirectToAction("Review", new { id });

            return Decision(id, false, comments);
        }
    }
}
