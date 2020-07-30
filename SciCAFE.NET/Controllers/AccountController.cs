using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;

        private readonly IMapper _mapper;

        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<User> signInManager, IMapper mapper, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
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
                _logger.LogInformation("{email} signed in", input.Email);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("{email} account is locked out", input.Email);
                return LocalRedirect(Url.Content("~/")); // XXX should display error message
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(input);
            }
        }

        public async Task<IActionResult> Logout()
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
        public IActionResult Register(RegistrationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _mapper.Map<User>(input);
            // user.Hash = BCrypt.Net.BCrypt.HashPassword(input.Password);
            // _userService.AddUser(user);
            // _userService.SaveChanges();
            _logger.LogInformation("{email} registered", input.Email);

            return View("RegisterStatus");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
