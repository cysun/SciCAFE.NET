using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly UserService _userService;
        private readonly EventService _eventService;
        private readonly RewardService _rewardService;
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager,
            UserService userService, EventService eventService, RewardService rewardService,
            ProgramService programService, IMapper mapper, ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _eventService = eventService;
            _rewardService = rewardService;
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUser(userId);
            ViewBag.EventsOrganizedCount = _eventService.GetEventsOrganizedCount(user.Id);
            ViewBag.EventsAttendedCount = _eventService.GetEventsAttendedCount(user.Id);
            ViewBag.RewardsProvidedCount = _rewardService.GetRewardsProvidedCount(user.Id);
            return View(user);
        }

        [HttpGet]
        public IActionResult Account()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUser(userId);
            var input = _mapper.Map<ProfileInputModel>(user);
            input.ProgramIds = user.UserPrograms.Select(p => p.ProgramId).ToList();

            ViewBag.User = user;
            ViewBag.Programs = _programService.GetPrograms().Select(p => new SelectListItem
            {
                Text = p.ShortName,
                Value = p.Id.ToString()
            });
            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> AccountAsync(ProfileInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUser(userId);

            if (!string.IsNullOrEmpty(input.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(input);
                }
                else
                {
                    await _signInManager.RefreshSignInAsync(user);
                    _logger.LogInformation("{user} changed password successfully.", user.Email);
                }
            }

            _mapper.Map(input, user);

            if (input.ProgramIds == null)
            {
                user.UserPrograms.Clear();
            }
            else
            {
                var currentProgramIds = user.UserPrograms.Select(e => e.ProgramId).ToHashSet();
                var programIdsToRemove = currentProgramIds.Except(input.ProgramIds).ToHashSet();
                var programIdsToAdd = input.ProgramIds.Except(currentProgramIds).ToList();
                user.UserPrograms.RemoveAll(p => programIdsToRemove.Contains(p.ProgramId));
                user.UserPrograms.AddRange(programIdsToAdd.Select(id => new UserProgram
                {
                    UserId = user.Id,
                    ProgramId = id
                }));
            }

            _userService.SaveChanges();
            _logger.LogInformation("{user} updated profile info.", user.Email);

            return RedirectToAction("Index");
        }
    }
}
