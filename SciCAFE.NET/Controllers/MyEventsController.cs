using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly FileService _fileService;
        private readonly EmailSender _emailSender;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<MyEventsController> _logger;

        public MyEventsController(EventService eventService, ProgramService programService,
            FileService fileService, EmailSender emailSender, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<MyEventsController> logger)
        {
            _eventService = eventService;
            _programService = programService;
            _fileService = fileService;
            _emailSender = emailSender;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_eventService.GetEventsByCreator(userId));
        }

        public IActionResult View(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            if (evnt.CreatorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(evnt);
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

            _logger.LogInformation("{user} created event {event}", User.Identity.Name, evnt.Id);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo", new { id = evnt.Id });
        }

        public IActionResult Clone(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var newEvent = evnt.Clone();
            newEvent.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _eventService.AddEvent(newEvent);
            _eventService.SaveChanges();

            _logger.LogInformation("{user} cloned event {event1} from {event2}",
                User.Identity.Name, newEvent.Id, evnt.Id);

            return RedirectToAction("Edit", new { id = newEvent.Id });
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanDeleteEvent);
            if (!authResult.Succeeded)
                return Forbid();

            if (evnt.Review?.IsApproved == true)
            {
                evnt.IsDeleted = true;
                _eventService.SaveChanges();
            }
            else
            {
                var files = evnt.EventAttachments.Select(a => a.File).ToList();
                _eventService.DeleteEvent(evnt);
                _eventService.SaveChanges();

                foreach (var file in files)
                {
                    if (!_eventService.IsAttachedToEvent(file.Id))
                        _fileService.DeleteFile(file.Id);
                }
            }

            _logger.LogInformation("{user} deleted event {event}", User.Identity.Name, id);

            return RedirectToAction("Index");
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

            _logger.LogInformation("{user} edited event {event}", User.Identity.Name, evnt.Id);

            return saveDraft ? RedirectToAction("Index") : RedirectToAction("AdditionalInfo", new { id = evnt.Id });
        }

        [HttpGet]
        public async Task<IActionResult> AdditionalInfoAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            ViewBag.Themes = _eventService.GetThemes();
            ViewBag.SelectedThemeIds = evnt.EventThemes.Select(t => t.ThemeId).ToList();
            return View(evnt);
        }

        [HttpGet]
        public async Task<IActionResult> AttachmentsAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            return View(evnt);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachmentsAsync(int id, IFormFile[] uploadedFiles)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            if (evnt.EventAttachments.Count + uploadedFiles.Length > 3)
                return BadRequest();

            foreach (var uploadedFile in uploadedFiles)
            {
                var file = _fileService.UploadFile(uploadedFile);
                evnt.EventAttachments.Add(new EventAttachment
                {
                    Event = evnt,
                    File = file
                });
                _logger.LogInformation("{user} uploaded file {file} to event {event}",
                    User.Identity.Name, file.Id, evnt.Id);
            }

            _eventService.SaveChanges();

            return Ok();
        }

        public async Task<IActionResult> DeleteAttachmentAsync(int id)
        {
            var attachment = _eventService.GetAttachment(id);
            if (attachment == null) return NotFound();

            var evnt = _eventService.GetEvent(attachment.EventId);
            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.EventAttachments.RemoveAll(a => a.Id == id);
            _eventService.SaveChanges();
            _logger.LogInformation("{user} removed file {file} from event {event}",
                User.Identity.Name, attachment.FileId, evnt.Id);

            if (!_eventService.IsAttachedToEvent(attachment.FileId))
            {
                _fileService.DeleteFile(attachment.FileId);
                _logger.LogInformation("{user} deleted file {file}", User.Identity.Name, attachment.FileId);
            }

            return RedirectToAction("Attachments", new { id = evnt.Id });
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

            if (evnt.SubmitDate == null)
            {
                evnt.SubmitDate = DateTime.Now;
                _eventService.SaveChanges();
                _logger.LogInformation("{user} submitted event {event}", User.Identity.Name, id);
            }
            var msg = _emailSender.CreateReviewEventMessage(evnt);
            if (msg != null) _ = _emailSender.SendAsync(msg);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PublishAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanReviewEvent);
            if (!authResult.Succeeded)
                return Submit(id);

            if (evnt.SubmitDate == null)
            {
                evnt.SubmitDate = DateTime.Now;
                evnt.Review = new Review
                {
                    IsApproved = true,
                    Timestamp = DateTime.Now,
                    ReviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                _eventService.SaveChanges();
                _logger.LogInformation("{user} published event {event}", User.Identity.Name, id);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AttendanceAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanManageAttendance);
            if (!authResult.Succeeded)
                return Forbid();

            ViewBag.Event = evnt;
            return View(_eventService.GetEventAttendances(id));
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

        private readonly string[] competencies = new string[]{
            "Written Communication",
            "Oral Communication",
            "Quantitative Reasoning",
            "Information Literacy",
            "Citical Thinking"
        };

        [HttpPut("MyEvents/{eventId}/CoreCompetency/{ccIndex}")]
        public async Task<IActionResult> SetCoreCompetencyAsync(int eventId, int ccIndex)
        {
            if (ccIndex < 0 || ccIndex >= competencies.Length)
                return BadRequest();

            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.CoreCompetency = string.IsNullOrEmpty(evnt.CoreCompetency) ? competencies[ccIndex]
                : $"{evnt.CoreCompetency}, {competencies[ccIndex]}";
            _eventService.SaveChanges();
            _logger.LogInformation("{user} set competency {competency} to event {event}",
                User.Identity.Name, ccIndex, eventId);

            return Ok();
        }

        [HttpDelete("MyEvents/{eventId}/CoreCompetency/{ccIndex}")]
        public async Task<IActionResult> RemoveCoreCompetencyAsync(int eventId, int ccIndex)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null || evnt.CoreCompetency == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEditEvent);
            if (!authResult.Succeeded)
                return Forbid();

            evnt.CoreCompetency = evnt.CoreCompetency.Replace(competencies[ccIndex] + ", ", null);
            evnt.CoreCompetency = evnt.CoreCompetency.Replace(", " + competencies[ccIndex], null);

            _eventService.SaveChanges();
            _logger.LogInformation("{user} removed competency {compentency} from event {event}",
                User.Identity.Name, ccIndex, eventId);

            return Ok();
        }

        [HttpPost("MyEvents/{eventId}/Attendances/{attendeeId}")]
        public async Task<IActionResult> AddAttendanceAsync(int eventId, string attendeeId)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanManageAttendance);
            if (!authResult.Succeeded)
                return Forbid();

            var attendance = _eventService.GetAttendance(eventId, attendeeId);
            if (attendance != null)
                return BadRequest();

            _eventService.AddAttendance(new Attendance
            {
                EventId = eventId,
                AttendeeId = attendeeId
            });
            _eventService.SaveChanges();
            _logger.LogInformation("{user} added attendee {attendee} to event {event}",
                User.Identity.Name, attendeeId, eventId);

            return Ok();
        }

        [HttpDelete("MyEvents/{eventId}/Attendances/{attendeeId}")]
        public async Task<IActionResult> RemoveAttendanceAsync(int eventId, string attendeeId)
        {
            var evnt = _eventService.GetEvent(eventId);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanManageAttendance);
            if (!authResult.Succeeded)
                return Forbid();

            var attendance = _eventService.GetAttendance(eventId, attendeeId);
            if (attendance == null)
                return BadRequest();

            _eventService.RemoveAttendance(attendance);
            _eventService.SaveChanges();
            _logger.LogInformation("{user} removed attendee {attendee} from event {event}",
                User.Identity.Name, attendeeId, eventId);

            return Ok();
        }
    }
}
