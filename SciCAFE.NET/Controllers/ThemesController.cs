using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Policy = Policy.IsAdministrator)]
    public class ThemesController : Controller
    {
        private readonly EventService _eventService;

        private readonly IMapper _mapper;
        private readonly ILogger<ThemesController> _logger;

        public ThemesController(EventService eventService, IMapper mapper, ILogger<ThemesController> logger)
        {
            _eventService = eventService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_eventService.GetThemes());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new Theme());
        }

        [HttpPost]
        public IActionResult Add(Theme input)
        {
            if (!ModelState.IsValid) return View(input);

            var theme = new Theme()
            {
                Name = input.Name,
                Description = input.Description
            };
            _eventService.AddTheme(theme);
            _eventService.SaveChanges();

            _logger.LogInformation("{user} added theme {theme}", User.Identity.Name, theme.Name);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var theme = _eventService.GetTheme(id);
            if (theme == null) return NotFound();

            return View(theme);
        }

        [HttpPost]
        public IActionResult Edit(int id, Theme input)
        {
            if (!ModelState.IsValid) return View(input);

            var theme = _eventService.GetTheme(id);
            theme.Name = input.Name;
            theme.Description = input.Description;
            _eventService.SaveChanges();

            _logger.LogInformation("{user} edited theme {theme}", User.Identity.Name, theme.Name);

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var theme = _eventService.GetTheme(id);
            theme.IsDeleted = true;
            _eventService.SaveChanges();

            _logger.LogInformation("{user} deleted theme {theme}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }
    }
}
