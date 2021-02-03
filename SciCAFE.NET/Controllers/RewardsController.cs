using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class RewardsController : Controller
    {
        private readonly RewardService _rewardService;

        private readonly IMapper _mapper;
        private readonly ILogger<RewardsController> _logger;

        public RewardsController(RewardService rewardService, IMapper mapper, ILogger<RewardsController> logger)
        {
            _rewardService = rewardService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_rewardService.GetCurrentRewards());
        }

        public IActionResult View(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null)
                return View("Error", new ErrorViewModel
                {
                    Message = "This reward does not exist."
                });

            if (reward.Review?.IsApproved != true)
                return Forbid();

            return View(reward);
        }
    }
}
