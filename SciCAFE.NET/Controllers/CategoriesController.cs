using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    }
}
