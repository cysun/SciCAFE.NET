﻿using System;
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

        private readonly ILogger<HomeController> _logger;

        public HomeController(EventService eventService, ILogger<HomeController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_eventService.GetUpcomingEvents());
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
