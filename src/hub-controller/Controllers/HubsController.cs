using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using HubController.Entities;
using HubController.Services;
using Microsoft.AspNetCore.Authorization;

namespace HubController.Controllers
{
    [Authorize(Policy = "HubCreator")]
    [Route("api/[controller]")]
    public class HubsController : ControllerBase
    {
        private readonly IHubService _hubService;

        public HubsController(IHubService hubService)
        {
            _hubService = hubService;
        }

        // GET api/hubs
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var hubs = await _hubService.GetAllHubs(HttpContext);
            return Ok(hubs);
        }

        // GET api/hubs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var hub = await _hubService.GetHubById(HttpContext, id);
            if(hub == null)
            {
                throw new KeyNotFoundException($"Hub {id} not found.");
            }
            return Ok(hub);
        }

        // POST api/hubs
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Hub hub)
        {
            if (hub == null || String.IsNullOrEmpty(hub.Name))
            {
                throw new ArgumentException("Invalid input! hub name is required");
            }
            var newHub = await _hubService.CreateHub(HttpContext, hub.Name);
            return CreatedAtAction(nameof(Get), new { Id = newHub.HubId }, newHub);
        }

        // DELETE api/hubs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _hubService.DeleteHub(HttpContext, id);

            return NoContent();
        }
    }
}
