using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class MyEventsController : Controller
    {
        private readonly EventService _eventService;
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<MyEventsController> _logger;

        public MyEventsController(EventService eventService, ProgramService programService,
            IMapper mapper, ILogger<MyEventsController> logger)
        {
            _eventService = eventService;
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_eventService.GetEventsByCreator(userId));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Programs = _programService.GetPrograms().Select(p => new SelectListItem
            {
                Text = p.ShortName,
                Value = p.Id.ToString()
            });
            ViewBag.Categories = _eventService.GetCategories().Select(c => new SelectListItem
            {
                Text = string.IsNullOrEmpty(c.AdditionalInfo) ? c.Name : $"{c.Name} ({c.AdditionalInfo})",
                Value = c.Id.ToString()
            });
            return View(new EventInputModel());
        }

        [HttpPost]
        public IActionResult Create(EventInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);

            var evnt = _mapper.Map<Event>(input);

            evnt.EventPrograms = input.ProgramIds.Select(id => new EventProgram
            {
                ProgramId = id,
                Event = evnt
            }).ToList();

            evnt.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _eventService.AddEvent(evnt);
            _eventService.SaveChanges();

            _logger.LogInformation("{user} created {event}", User.Identity.Name, evnt.Name);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var input = _mapper.Map<EventInputModel>(evnt);
            input.ProgramIds = evnt.EventPrograms.Select(p => p.ProgramId).ToList();

            ViewBag.Programs = _programService.GetPrograms().Select(p => new SelectListItem
            {
                Text = p.ShortName,
                Value = p.Id.ToString()
            });
            ViewBag.Categories = _eventService.GetCategories().Select(c => new SelectListItem
            {
                Text = string.IsNullOrEmpty(c.AdditionalInfo) ? c.Name : $"{c.Name} ({c.AdditionalInfo})",
                Value = c.Id.ToString()
            });

            return View(input);
        }

        [HttpPost]
        public IActionResult Edit(int id, EventInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);

            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            _mapper.Map(input, evnt);

            var currentProgramIds = evnt.EventPrograms.Select(e => e.ProgramId).ToHashSet();
            var programIdsToRemove = currentProgramIds.Except(input.ProgramIds).ToHashSet();
            var programIdsToAdd = input.ProgramIds.Except(currentProgramIds).ToList();
            evnt.EventPrograms.RemoveAll(p => programIdsToRemove.Contains(p.ProgramId));
            evnt.EventPrograms.AddRange(programIdsToAdd.Select(id => new EventProgram
            {
                EventId = evnt.Id,
                ProgramId = id
            }));

            _eventService.SaveChanges();

            _logger.LogInformation("{user} edited {event}", User.Identity.Name, evnt.Name);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo");
        }
    }
}
