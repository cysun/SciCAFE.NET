using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SciCAFE.NET.Models;
using SciCAFE.NET.Services;

namespace SciCAFE.NET.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventService _eventService;

        private readonly IMapper _mapper;
        private readonly ILogger<EventsController> _logger;

        public EventsController(EventService eventService, IMapper mapper, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult View(int id)
        {
            var evnt = _eventService.GetEvent(id);
            if (evnt == null)
                return View("Error", new ErrorViewModel
                {
                    Message = "This event does not exist."
                });

            if (evnt.Review?.IsApproved != true)
                return Forbid();

            return View(evnt);
        }

        [HttpGet("Events/Start/{startTime}/End/{endTime}")]
        public List<EventViewModel> Events(DateTime startTime, DateTime endTime)
        {
            return _mapper.Map<List<Event>, List<EventViewModel>>(_eventService.GetEvents(startTime, endTime));
        }

        [HttpGet("Events/Search")]
        public List<EventViewModel> Search([FromQuery] string q)
        {
            return _mapper.Map<List<Event>, List<EventViewModel>>(_eventService.SearchEvents(q).Take(10).ToList());
        }
    }
}
