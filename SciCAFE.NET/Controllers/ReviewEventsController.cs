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
    [Authorize(Policy = Policy.IsEventReviewer)]
    public class ReviewEventsController : Controller
    {
        private readonly EventService _eventService;
        private readonly EmailSender _emailSender;

        private readonly ILogger<ReviewEventsController> _logger;

        public ReviewEventsController(EventService eventService, EmailSender emailSender,
            IMapper mapper, ILogger<ReviewEventsController> logger)
        {
            _eventService = eventService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_eventService.GetUnreviewedEvents());
        }

        public IActionResult Review(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            if (evnt.Review?.IsApproved != null)
                return View("Status", new StatusViewModel
                {
                    Subject = "Event Already Reviewed",
                    Message = "This event has already been reviewed."
                });

            return View(evnt);
        }

        private IActionResult Decision(int id, bool IsApproved, string comments)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            if (evnt.Review?.IsApproved != null)
                return View("Status", new StatusViewModel
                {
                    Subject = "Event Already Reviewed",
                    Message = "This event has already been reviewed."
                });

            evnt.Review = new Review
            {
                IsApproved = IsApproved,
                Comments = comments,
                Timestamp = DateTime.Now,
                ReviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _eventService.SaveChanges();

            _logger.LogInformation("{user} reviewed event {event} with approval={decision}",
                User.Identity.Name, evnt.Id, IsApproved);

            var msg = _emailSender.CreateEventReviewedMessage(evnt);
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
