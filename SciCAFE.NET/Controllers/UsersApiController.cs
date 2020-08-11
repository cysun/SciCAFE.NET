using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly UserService _userService;

        private readonly IMapper _mapper;

        public UsersApiController(UserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("Users/PrefixSearch")]
        public List<UserViewModel> PrefixSearch([FromQuery] string q)
        {
            return _mapper.Map<List<User>, List<UserViewModel>>(_userService.SearchUsersByPrefix(q));
        }
    }
}
