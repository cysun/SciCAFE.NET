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
using Org.BouncyCastle.Asn1;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class MyEventsController : Controller
    {
        private readonly EventService _eventService;
        private readonly ProgramService _programService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<MyEventsController> _logger;

        public MyEventsController(EventService eventService, ProgramService programService,
            IAuthorizationService authorizationService, IMapper mapper, ILogger<MyEventsController> logger)
        {
            _eventService = eventService;
            _programService = programService;
            _authorizationService = authorizationService;
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

            if (input.ProgramIds != null)
            {
                evnt.EventPrograms = input.ProgramIds.Select(id => new EventProgram
                {
                    ProgramId = id,
                    Event = evnt
                }).ToList();
            }

            evnt.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _eventService.AddEvent(evnt);
            _eventService.SaveChanges();

            _logger.LogInformation("{user} created event {event}", User.Identity.Name, evnt.Name);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo", new { id = evnt.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

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
        public async Task<IActionResult> EditAsync(int id, EventInputModel input, bool saveDraft = false)
        {
            if (!ModelState.IsValid) return View(input);

            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            _mapper.Map(input, evnt);

            if (input.ProgramIds == null)
            {
                evnt.EventPrograms.Clear();
            }
            else
            {
                var currentProgramIds = evnt.EventPrograms.Select(e => e.ProgramId).ToHashSet();
                var programIdsToRemove = currentProgramIds.Except(input.ProgramIds).ToHashSet();
                var programIdsToAdd = input.ProgramIds.Except(currentProgramIds).ToList();
                evnt.EventPrograms.RemoveAll(p => programIdsToRemove.Contains(p.ProgramId));
                evnt.EventPrograms.AddRange(programIdsToAdd.Select(id => new EventProgram
                {
                    EventId = evnt.Id,
                    ProgramId = id
                }));
            }

            _eventService.SaveChanges();

            _logger.LogInformation("{user} edited event {event}", User.Identity.Name, evnt.Name);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo");
        }

        [HttpGet]
        public IActionResult AdditionalInfo(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            ViewBag.Themes = _eventService.GetThemes();
            ViewBag.SelectedThemeIds = evnt.EventThemes.Select(t => t.ThemeId).ToList();
            return View(evnt);
        }

        [HttpGet]
        public IActionResult Summary(int id)
        {
            return View(_eventService.GetEvent(id));
        }

        public IActionResult Submit(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            evnt.SubmitDate = DateTime.Now;
            _eventService.SaveChanges();
            _logger.LogInformation("{user} submitted event {event}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PublishAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanReviewEvent);
            if (!authResult.Succeeded)
                return Submit(id);

            evnt.SubmitDate = DateTime.Now;
            evnt.Review = new Review
            {
                IsApproved = true,
                Timestamp = DateTime.Now,
                ReviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _eventService.SaveChanges();
            _logger.LogInformation("{user} published event {event}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        [HttpPost("MyEvents/{eventId}/Themes/{themeId}")]
        public async Task<IActionResult> AddThemeAsync(int eventId, int themeId)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            if (evnt.EventThemes.Count >= 3)
                return BadRequest();

            if (evnt.EventThemes.Find(t => t.ThemeId == themeId) == null)
            {
                evnt.EventThemes.Add(new EventTheme
                {
                    EventId = eventId,
                    ThemeId = themeId
                });
                _eventService.SaveChanges();
                _logger.LogInformation("{user} added theme {theme} to event {event}", User.Identity.Name, themeId, eventId);
            }

            return Ok();
        }

        [HttpDelete("MyEvents/{eventId}/Themes/{themeId}")]
        public async Task<IActionResult> RemoveThemeAsync(int eventId, int themeId)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.EventThemes.RemoveAll(t => t.ThemeId == themeId);
            _eventService.SaveChanges();
            _logger.LogInformation("{user} removed theme {theme} to event {event}", User.Identity.Name, themeId, eventId);

            return Ok();
        }

        [HttpPut("MyEvents/{eventId}/CoreCompetency/{ccIndex}")]
        public async Task<IActionResult> SetCoreCompetencyAsync(int eventId, int ccIndex)
        {
            string[] competencies = new string[]{"Written Communication", "Oral Communication", "Quantitative Reasoning",
            "Information Literacy", "Citical Thinking"};

            if (ccIndex < 0 || ccIndex >= competencies.Length)
                return BadRequest();

            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.CoreCompetency = competencies[ccIndex];
            _eventService.SaveChanges();
            _logger.LogInformation("{user} set competency {competency} to event {event}", User.Identity.Name, ccIndex, eventId);

            return Ok();
        }

        [HttpDelete("MyEvents/{eventId}/CoreCompetency/{ccIndex}")]
        public async Task<IActionResult> RemoveCoreCompetencyAsync(int eventId)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.CoreCompetency = null;
            _eventService.SaveChanges();
            _logger.LogInformation("{user} removed competency from event {event}", User.Identity.Name, eventId);

            return Ok();
        }
    }
}
