using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [Authorize(Policy = Policy.IsAdministrator)]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly UserManager<User> _userManager;

        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, UserManager<User> userManager,
            IMapper mapper, ILogger<UsersController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_userService.GetUsers());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new NewUserInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(NewUserInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _mapper.Map<User>(input);
            user.UserName = input.Email;
            user.EmailConfirmed = true;
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                List<Claim> claims = new List<Claim>();
                if (user.IsAdministrator)
                    claims.Add(new Claim(ClaimType.IsAdministrator, "True"));
                if (user.IsEventOrganizer)
                    claims.Add(new Claim(ClaimType.IsEventOrganizer, "True"));
                if (user.IsRewardProvider)
                    claims.Add(new Claim(ClaimType.IsRewardProvider, "True"));
                if (claims.Count > 0)
                    await _userManager.AddClaimsAsync(user, claims);

                _logger.LogInformation("{user} created account for {newUser}", User.Identity.Name, input.Email);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(input);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var user = _userService.GetUser(id);
            if (user == null) return NotFound();

            return View(_mapper.Map<EditUserInputModel>(user));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(string id, EditUserInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _userService.GetUser(id);

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                var result = await ChangePaswordAsync(user, input.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("Password", error.Description);
                    return View(input);
                }
            }

            await UpdateUserClaims(user, input);

            _mapper.Map<EditUserInputModel, User>(input, user);
            _userService.SaveChanges();

            _logger.LogInformation("{user} edited account {account}", User.Identity.Name, input.Email);

            return RedirectToAction(nameof(Index));
        }

        private async Task<IdentityResult> ChangePaswordAsync(User user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        private async Task UpdateUserClaims(User user, EditUserInputModel input)
        {
            List<Claim> claimsToAdd = new List<Claim>();
            List<Claim> claimsToRemove = new List<Claim>();
            if (user.IsAdministrator != input.IsAdministrator)
            {
                var claim = new Claim(ClaimType.IsAdministrator, "True");
                if (user.IsAdministrator) claimsToRemove.Add(claim);
                else claimsToAdd.Add(claim);
            }
            if (user.IsEventOrganizer != input.IsEventOrganizer)
            {
                var claim = new Claim(ClaimType.IsEventOrganizer, "True");
                if (user.IsEventOrganizer) claimsToRemove.Add(claim);
                else claimsToAdd.Add(claim);
            }
            if (user.IsRewardProvider != input.IsRewardProvider)
            {
                var claim = new Claim(ClaimType.IsRewardProvider, "True");
                if (user.IsRewardProvider) claimsToRemove.Add(claim);
                else claimsToAdd.Add(claim);
            }

            if (claimsToAdd.Count > 0)
                await _userManager.AddClaimsAsync(user, claimsToAdd);
            if (claimsToRemove.Count > 0)
                await _userManager.RemoveClaimsAsync(user, claimsToRemove);
        }
    }
}
