using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager,
            IMapper mapper, ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> AccountAsync()
        {
            ViewBag.User = await _userManager.GetUserAsync(User);
            return View(new PasswordInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> AccountAsync(PasswordInputModel input)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(input);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("{user} changed password successfully.", user.Email);

            return RedirectToAction("Account", new { success = "true" });
        }
    }
}
