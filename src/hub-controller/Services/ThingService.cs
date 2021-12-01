using HubController.Entities;
using HubController.Exceptions;
using HubController.Models.DAO;
using HubController.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public class ThingService : IThingService
    {
        private readonly IThingIdGenerator _thingIdGenerator;
        private readonly IHubService _hubService;

        public ThingService(IThingIdGenerator thingIdGenerator, IHubService hubService)
        {
            _thingIdGenerator = thingIdGenerator;
            _hubService = hubService;
        }

        public async Task<Thing> Create(HttpContext httpContext, Guid hubId, ThingDAO thingDao)
        {
            var hub = await _hubService.GetHubById(httpContext, hubId); 
            if(hub == null)
            {
                throw new KeyNotFoundException($"Hub {hubId} not found.");
            }

            // check idempotency
            if(hub.Things == null)
            {
                hub.Things = new List<Thing>();
            }
            var existingThing = hub.Things.FirstOrDefault(thing => thing.Name == thingDao.Name);
            if(existingThing != null)
            {
                return existingThing;
            }

            // check limit
            var allowedNumberOfThingsPerHub = Int32.Parse(Environment.GetEnvironmentVariable("THINGS_ALLOWED_PER_HUB") ?? Constants.DEFAULT_THINGS_ALLOWED_PER_HUB);
            if(hub.Things.Count >= allowedNumberOfThingsPerHub)
            {
                throw new LimitExceededException($"Cannot create more things for Hub {hubId}. Limit reached.");
            }

            // Add thing to hub and save
            var thing = Thing.Create(thingDao.Name, thingDao.Description, _thingIdGenerator.Generate());
            hub.Things.Add(thing);
            await _hubService.SaveHub(hub);
            return thing;
        }

        public async Task DeleteThing(HttpContext httpContext, Guid hubId, string id)
        {
            var hub = await _hubService.GetHubById(httpContext, hubId);
            if (hub == null || hub.Things == null)
            {
                return;
            }
            var existingThing = hub.Things.FirstOrDefault(thing => thing.ThingId == id);
            if(existingThing == null)
            {
                return;
            }

            hub.Things.Remove(existingThing);
            await _hubService.SaveHub(hub);
        }

        public async Task<Thing> GetThingById(HttpContext httpContext, Guid hubId, string thingId)
        {
            var hub = await _hubService.GetHubById(httpContext, hubId);
            if (hub == null)
            {
                return null;
            }
            return hub.Things.FirstOrDefault(thing => thing.ThingId == thingId);
        }

        public async Task<Thing> UpdateStatus(HttpContext httpContext, Guid hubId, string id, ThingStatusDAO thingStatusDAO)
        {
            var hub = await _hubService.GetHubById(httpContext, hubId);
            if (hub == null)
            {
                return null;
            }

            var existingThing = hub.Things.FirstOrDefault(thing => thing.ThingId == id);
            if (existingThing == null)
            {
                return null;
            }
            
            if(existingThing.Status == thingStatusDAO.Status)
            {
                return existingThing;
            }
            
            existingThing.Status = thingStatusDAO.Status;
            existingThing.UpdatedAt = DateTime.UtcNow;
            await _hubService.SaveHub(hub);
            return existingThing;
        }
    }
}
