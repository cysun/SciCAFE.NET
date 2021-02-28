using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class EmailController : Controller
    {
        private readonly EventService _eventService;
        private readonly RewardService _rewardService;
        private readonly EmailSender _emailSender;

        private readonly IAuthorizationService _authorizationService;

        private readonly ILogger<EmailController> _logger;

        public EmailController(EventService eventService, RewardService rewardService, EmailSender emailSender,
            IAuthorizationService authorizationService, ILogger<EmailController> logger)
        {
            _eventService = eventService;
            _rewardService = rewardService;
            _emailSender = emailSender;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> AttendeesAsync(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEmail);
            if (!authResult.Succeeded)
                return Forbid();

            return View("Compose", new EmailInputModel
            {
                To = $"Attendees of {evnt.Name}",
                RedirectUrl = $"{Request.PathBase}/MyEvents/Attendance/{id}"
            });
        }

        [HttpPost]
        public async Task<IActionResult> AttendeesAsync(int id, EmailInputModel input)
        {
            if (!ModelState.IsValid) return View("Compose", input);

            var evnt = _eventService.GetEvent(id);
            if (evnt == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, evnt, Policy.CanEmail);
            if (!authResult.Succeeded)
                return Forbid();

            var recipients = _eventService.GetEventAttendances(id).Select(a => a.Attendee).ToList();
            var msg = _emailSender.CreateMessage(User, recipients, input.Subject, input.Content);
            if (msg != null) _ = _emailSender.SendAsync(msg);

            _logger.LogInformation("{user} emailed to the attendees of event {event}", User.Identity.Name, id);

            return Redirect(input.RedirectUrl);
        }

        [HttpGet]
        public async Task<IActionResult> RewardeesAsync(int id)
        {
            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEmail);
            if (!authResult.Succeeded)
                return Forbid();

            return View("Compose", new EmailInputModel
            {
                To = $"Rewardees of {reward.Name}",
                RedirectUrl = $"{Request.PathBase}/MyRewards/Rewardees/{id}"
            });
        }

        [HttpPost]
        public async Task<IActionResult> RewardeesAsync(int id, EmailInputModel input)
        {
            if (!ModelState.IsValid) return View("Compose", input);

            var reward = _rewardService.GetReward(id);
            if (reward == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, reward, Policy.CanEmail);
            if (!authResult.Succeeded)
                return Forbid();

            var recipients = _rewardService.GetRewardees(id).Where(r => r.RequirementsMet)
                .Select(r => new User
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Email = r.Email
                }).ToList();
            var msg = _emailSender.CreateMessage(User, recipients, input.Subject, input.Content);
            if (msg != null) _ = _emailSender.SendAsync(msg);

            _logger.LogInformation("{user} emailed to the rewardees of reward {reward}", User.Identity.Name, id);

            return Redirect(input.RedirectUrl);
        }
    }
}
