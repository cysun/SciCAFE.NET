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
    public class CategoriesController : Controller
    {
        private readonly EventService _eventService;

        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(EventService eventService, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _eventService = eventService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_eventService.GetCategories());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new Category());
        }

        [HttpPost]
        public IActionResult Add(Category input)
        {
            if (!ModelState.IsValid) return View(input);

            var category = new Category()
            {
                Name = input.Name,
                AdditionalInfo = input.AdditionalInfo
            };
            _eventService.AddCategory(category);
            _eventService.SaveChanges();

            _logger.LogInformation("{user} added category {category}", User.Identity.Name, category.Id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _eventService.GetCategory(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category input)
        {
            if (!ModelState.IsValid) return View(input);

            var category = _eventService.GetCategory(id);
            category.Name = input.Name;
            category.AdditionalInfo = input.AdditionalInfo;
            _eventService.SaveChanges();

            _logger.LogInformation("{user} edited category {category}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = _eventService.GetCategory(id);
            category.IsDeleted = true;
            _eventService.SaveChanges();

            _logger.LogInformation("{user} deleted category {category}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }
    }
}
