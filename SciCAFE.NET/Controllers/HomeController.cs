using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventService _eventService;
        private readonly RewardService _rewardService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(EventService eventService, RewardService rewardService, ILogger<HomeController> logger)
        {
            _eventService = eventService;
            _rewardService = rewardService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View((_eventService.GetUpcomingEvents(), _rewardService.GetRecentRewards()));
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                Message = "An error occurred"
            });
        }
    }
}
