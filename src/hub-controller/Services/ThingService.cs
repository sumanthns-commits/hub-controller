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
        private readonly IThingRepository _thingRepository;
        private readonly IThingIdGenerator _thingIdGenerator;
        private readonly IHubService _hubService;

        public ThingService(IThingRepository thingRepository, IThingIdGenerator thingIdGenerator, IHubService hubService)
        {
            _thingRepository = thingRepository;
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

            var things = await _thingRepository.FindAll(hubId);
            // check idempotency
            var existingThing = things.FirstOrDefault(thing => thing.Name == thingDao.Name);
            if(existingThing != null)
            {
                return existingThing;
            }

            // check limit
            var allowedNumberOfThingsPerHub = Int32.Parse(Environment.GetEnvironmentVariable("THINGS_ALLOWED_PER_HUB") ?? Constants.DEFAULT_THINGS_ALLOWED_PER_HUB);
            if(things.Count >= allowedNumberOfThingsPerHub)
            {
                throw new LimitExceededException($"Cannot create more things for Hub {hubId}. Limit reached.");
            }

            return await _thingRepository.Create(hubId, thingDao.Name, thingDao.Description, _thingIdGenerator.Generate());
        }

        public Task<Thing> GetThingById(HttpContext httpContext, Guid hubId, string id)
        {
            throw new NotImplementedException();
        }
    }
}
