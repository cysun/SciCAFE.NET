using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        private readonly IMapper _mapper;

        private readonly ILogger<AccountController> _logger;

        public AccountController(UserService userService, IMapper mapper, ILogger<AccountController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(string email, string password, string returnUrl)
        {
            var user = _userService.Authenticate(email, password);
            if (user == null)
                return RedirectToAction(nameof(Login));

            var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), authProperties);

            return string.IsNullOrWhiteSpace(returnUrl) ? RedirectToAction("Index", "Home")
                : (IActionResult)LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
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
            user.Hash = BCrypt.Net.BCrypt.HashPassword(input.Password);
            _userService.AddUser(user);
            _userService.SaveChanges();
            _logger.LogInformation("{email} registered", input.Email);

            return View("RegisterStatus");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
