using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SciCAFE.NET.Models;

namespace SciCAFE.NET.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegistrationInputModel, User>();
            CreateMap<NewUserInputModel, User>();
            CreateMap<User, EditUserInputModel>();
            CreateMap<User, UserViewModel>();
            CreateMap<EditUserInputModel, User>();
            CreateMap<ProgramInputModel, Models.Program>();
            CreateMap<Models.Program, ProgramInputModel>();
            CreateMap<EventInputModel, Event>();
            CreateMap<Event, EventInputModel>();
            CreateMap<Event, EventViewModel>();
            CreateMap<RewardInputModel, Reward>();
        }
    }
}
