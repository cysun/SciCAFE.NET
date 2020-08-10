using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailSender _emailSender;

        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            EmailSender emailSender, IMapper mapper, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginInputModel input, string returnUrl)
        {
            if (!ModelState.IsValid) return View(input);

            returnUrl = returnUrl ?? Url.Content("~/");
            var result = await _signInManager.PasswordSignInAsync(input.Email, input.Password, input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("{user} signed in", input.Email);
                return LocalRedirect(returnUrl);
            }
            else
            {
                _logger.LogInformation("{user} failed to log in. LockedOut: {lockedOut}; NotAllowed: {notAllowed}",
                    input.Email, result.IsLockedOut, result.IsNotAllowed);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(input);
            }
        }

        public async Task<IActionResult> LogoutAsync()
        {
            var name = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("{user} signed out", name);
            return LocalRedirect(Url.Content("~/"));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegistrationInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegistrationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _mapper.Map<User>(input);
            user.UserName = input.Email;
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("New account for {user} created", input.Email);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var link = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    code = code
                });
                var msg = _emailSender.CreateEmailVerificationMessage(user, link);
                _ = _emailSender.SendAsync(msg);

                return View("RegisterStatus", user.Email);
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(input);
            }
        }

        public async Task<IActionResult> ConfirmEmailAsync(string userId, string code)
        {
            if (userId == null || code == null)
                return LocalRedirect("~/");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
                return View("Status", new StatusViewModel
                {
                    Message = "Thank you for confirming your email. Your account is now activated."
                });
            else
                return View("Error", new ErrorViewModel
                {
                    Message = "Sorry we cannot verify your email."
                });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
