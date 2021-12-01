using HubController.Entities;
using HubController.Models.DAO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IThingService
    {
        public Task<Thing> Create(HttpContext httpContext, Guid hubId, ThingDAO thingDao);
        public Task<Thing> GetThingById(HttpContext httpContext, Guid hubId, string id);
        public Task DeleteThing(HttpContext httpContext, Guid hubId, string id);
        public Task<Thing> UpdateStatus(HttpContext httpContext, Guid hubId, string id, ThingStatusDAO thingStatusDAO);
    }
}
