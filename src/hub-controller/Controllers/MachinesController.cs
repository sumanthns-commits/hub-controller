using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using HubController.Services;
using AutoMapper;
using HubController.Models.DTO;
using Amazon.Lambda.Core;
using HubController.Entities;

namespace HubController.Controllers
{
    [Route("machineapi/[controller]")]
    public class MachinesController : ControllerBase
    {
        private IHubService _hubService;
        private readonly IMapper _mapper;

        public MachinesController(IHubService hubService, IMapper mapper)
        {
            _hubService = hubService;
            _mapper = mapper;
        }

        // GET machineapi/machines/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            LambdaLogger.Log($"**************************Identity**** {HttpContext.User.Identity.Name}");

            //var hub = await _hubService.GetHubById(HttpContext, id);
            //if (hub == null)
            //{
            //    throw new KeyNotFoundException($"Hub {id} not found.");
            //}
            var hub = new Hub() { Things = new List<Thing>() { new Thing() { ThingId = "sample", Status = Constants.THING_OFF } } };
            return Ok(_mapper.Map<MachineDTO>(hub));
        }
    }
}
