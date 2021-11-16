using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;

using HubController.Entities;
using Amazon.DynamoDBv2.DocumentModel;
using System.Linq;
using HubController.Services;
using Microsoft.AspNetCore.Authorization;

namespace HubController.Controllers
{
    [Authorize(Policy = "HubCreator")]
    [Route("api/[controller]")]
    public class HubsController : ControllerBase
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;
        private readonly IUserService _userService;

        public HubsController(IAmazonDynamoDB client, IUserService userService)
        {
            _client = client;
            _context = new DynamoDBContext(client);
            _userService = userService;
        }

        // GET api/hubs
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = _userService.GetUserId(HttpContext);
            var primaryKey = $"user_hub_${userId}";
            var hubs = new List<Hub>();
            var search = _context.QueryAsync<Hub>(primaryKey);
            do
            {
                hubs.AddRange(await search.GetNextSetAsync());
            } while (!search.IsDone);
            return Ok(hubs);
        }

        // GET api/hubs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var userId = _userService.GetUserId(HttpContext);
            var primaryKey = $"user_hub_${userId}";
            var hub = await _context.LoadAsync<Hub>(primaryKey, id);
            return Ok(hub);
        }

        // POST api/hubs
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Hub hub)
        {
            if (hub == null)
            {
                throw new ArgumentException("Invalid input! Book not informed");
            }
            var userId = _userService.GetUserId(HttpContext);
            hub.UserId = $"user_hub_${userId}";
            hub.HubId = Guid.NewGuid();
            hub.CreatedAt = DateTime.Now;

            await _context.SaveAsync<Hub>(hub);
            return CreatedAtAction(nameof(Get), new { Id = hub.HubId }, hub);
        }

        // DELETE api/hubs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Delete the book.
            var userId = _userService.GetUserId(HttpContext);
            var primaryKey = $"user_hub_${userId}";
            await _context.DeleteAsync<Hub>(primaryKey, id);

            // Try to retrieve deleted hub. It should return null.
            var operationConfig = new DynamoDBOperationConfig
            {
                ConsistentRead = true
            };
            Hub deletedHub = await _context.LoadAsync<Hub>(primaryKey, id, operationConfig);

            if (deletedHub != null)
            {
                throw new Exception($"Could not delete hub {id}! Try again later.");
            }

            return NoContent();
        }
    }
}
