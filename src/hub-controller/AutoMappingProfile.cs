using HubController.Entities;
using HubController.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HubController.Models.DAO;
using AutoMapper;

namespace HubController
{
    public class AutoMappingProfile: Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<Hub, HubDTO>();
            CreateMap<HubDAO, Hub>();
            CreateMap<Thing, ThingDTO>();
            CreateMap<ThingDAO, Thing>();
        }
    }
}
