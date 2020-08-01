using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Policy = Policy.IsAdministrator)]
    public class ProgramsController : Controller
    {
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<ProgramsController> _logger;

        public ProgramsController(ProgramService programService, IMapper mapper, ILogger<ProgramsController> logger)
        {
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_programService.GetPrograms());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new ProgramInputModel());
        }

        [HttpPost]
        public IActionResult Add(ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _mapper.Map<Models.Program>(input);
            _programService.AddProgram(program);
            _programService.SaveChanges();

            _logger.LogInformation("{user} added program {program}", User.Identity.Name, program.ShortName);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            return View(_mapper.Map<ProgramInputModel>(program));
        }

        [HttpPost]
        public IActionResult Edit(int id, ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _programService.GetProgram(id);
            _mapper.Map(input, program);
            _programService.SaveChanges();

            _logger.LogInformation("{user} edited program {program}", User.Identity.Name, program.ShortName);

            return RedirectToAction("Index");
        }
    }
}
