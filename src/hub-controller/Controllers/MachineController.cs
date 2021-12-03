using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using HubController.Services;
using AutoMapper;
using HubController.Models.DTO;
using Amazon.Lambda.Core;
using HubController.Entities;
using System.Linq;

namespace HubController.Controllers
{
    [Route("machineapi/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly IHubService _hubService;
        private readonly IMapper _mapper;

        public MachineController(IHubService hubService, IMapper mapper)
        {
            _hubService = hubService;
            _mapper = mapper;
        }

        // GET machineapi/machine
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var hubIdClaim = HttpContext.User.Claims.First(claim => claim.Type == Constants.HUB_ID_CLAIM_TYPE);
            var hubId = Guid.Parse(hubIdClaim.Value);

            var hub = await _hubService.GetHubById(HttpContext, hubId);
            if (hub == null)
            {
                throw new KeyNotFoundException($"Hub {hubId} not found.");
            }

            return Ok(_mapper.Map<MachineDTO>(hub));
        }
    }
}
