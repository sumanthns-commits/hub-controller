using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using HubController.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using HubController.Models.DTO;
using HubController.Models.DAO;
using HubController.Entities;

namespace HubController.Controllers
{
    [Authorize(Policy = "HubCreator")]
    [Route("api/hubs/{hubId}/[controller]")]
    public class ThingsController : ControllerBase
    {
        private readonly IThingService _thingService;
        private readonly IMapper _mapper;

        public ThingsController(IThingService thingService, IMapper mapper)
        {
            _thingService = thingService;
            _mapper = mapper;
        }

        // GET api/hubs/{hubId}/things
        //[HttpGet]
        //public async Task<IActionResult> List()
        //{
        //    var hubs = await _hubService.GetAllHubs(HttpContext);
        //    return Ok(_mapper.Map<List<HubDTO>>(hubs));
        //}

        // GET api/hubs/{hubId}/things/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid hubId, String id)
        {
            var thing = await _thingService.GetThingById(HttpContext, hubId, id);
            if (thing == null)
            {
                throw new KeyNotFoundException($"Thing {id} not found.");
            }
            return Ok(_mapper.Map<ThingDTO>(thing));
        }

        // POST api/hubs/{hubId}/things
        [HttpPost]
        public async Task<IActionResult> Post([FromRoute] Guid hubId, [FromBody] ThingDAO thingDao)
        {
            if (thingDao == null || String.IsNullOrEmpty(thingDao.Name))
            {
                throw new ArgumentException("Invalid input! thing name is required");
            }
            var newThing = await _thingService.Create(HttpContext, hubId, thingDao);
            return CreatedAtAction(nameof(Get), new { HubId = hubId, Id = newThing.ThingId }, _mapper.Map<ThingDTO>(newThing));
        }

        // DELETE api/hubs/{hubId}/things/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid hubId, string id)
        {
            await _thingService.DeleteThing(HttpContext, hubId, id);

            return NoContent();
        }

        // PATCH api/hubs/{hubId}/things/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(Guid hubId, string id, [FromBody] ThingStatusDAO thingStatusDAO)
        {
            if (String.IsNullOrEmpty(thingStatusDAO.Status) || !Thing.ValidStatuses.Contains(thingStatusDAO.Status)) { 
                throw new ArgumentException($"Thing status should be off|on");
            }

            var thing = await _thingService.UpdateStatus(HttpContext, hubId, id, thingStatusDAO);
            if (thing == null)
            {
                throw new KeyNotFoundException($"Thing {id} not found.");
            }

            return Ok(_mapper.Map<ThingDTO>(thing));
        }
    }
}
