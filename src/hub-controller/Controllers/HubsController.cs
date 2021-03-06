using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using HubController.Services;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using HubController.Models.DTO;
using HubController.Models.DAO;

namespace HubController.Controllers
{
    [Authorize(Policy = "HubAdmin")]
    [Route("api/[controller]")]
    public class HubsController : ControllerBase
    {
        private readonly IHubService _hubService;
        private readonly IMapper _mapper;

        public HubsController(IHubService hubService, IMapper mapper)
        {
            _hubService = hubService;
            _mapper = mapper;
        }

        // GET api/hubs
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var hubs = await _hubService.GetAllHubs(HttpContext);
            return Ok(_mapper.Map<List<HubDTO>>(hubs));
        }

        // GET api/hubs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var hub = await _hubService.GetHubById(HttpContext, id);
            if (hub == null)
            {
                throw new KeyNotFoundException($"Hub {id} not found.");
            }
            return Ok(_mapper.Map<HubDTO>(hub));
        }

        // POST api/hubs
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HubDAO hubDAO)
        {
            if (hubDAO == null || String.IsNullOrEmpty(hubDAO.Name) || String.IsNullOrEmpty(hubDAO.Password))
            {
                throw new ArgumentException("Invalid input! hub name and password is required");
            }
            var newHub = await _hubService.CreateHub(HttpContext, hubDAO);
            return CreatedAtAction(nameof(Get), new { Id = newHub.HubId }, _mapper.Map<HubDTO>(newHub));
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
