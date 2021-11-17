using Amazon.DynamoDBv2.DataModel;
using HubController.Entities;
using HubController.Exceptions;
using HubController.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public class HubService : IHubService
    {
        private readonly IHubRepository _hubRepository;
        private readonly IUserService _userService;

        public HubService(IHubRepository hubRepoitory, IUserService userService)
        {
            _hubRepository = hubRepoitory;
            _userService = userService;
        }

        public async Task<Hub> CreateHub(HttpContext httpContext, String name)
        {
            var allowedNumberOfHubsPerUser = Int32.Parse(Environment.GetEnvironmentVariable("HUBS_ALLOWER_PER_USER") ?? Constants.HUBS_ALLOWED_PER_USER);
            var userId = _userService.GetUserId(httpContext);
            var hubs = await _hubRepository.FindAll(userId);
            
            // Hubs are idempotent by name
            var existingHub = hubs.FirstOrDefault(h => h.Name.Equals(name));
            if (existingHub != null)
            {
                return existingHub;
            }
            
            // Check if user has already exceeded limit
            if (hubs.Count >= allowedNumberOfHubsPerUser)
            {
                throw new LimitExceededException("Reached max quota");
            }

            return await _hubRepository.Create(userId, name);
        }

        public async Task DeleteHub(HttpContext httpContext, Guid id)
        {
            // Delete hub.
            var userId = _userService.GetUserId(httpContext);
            await _hubRepository.Delete(userId, id);

            // Try to retrieve deleted hub. It should return null.
            Hub deletedHub = await _hubRepository.FindConsistently(userId, id);
            if (deletedHub != null)
            {
                throw new Exception($"Could not delete hub {id}! Try again later.");
            }
        }

        public Task<List<Hub>> GetAllHubs(HttpContext httpContext)
        {
            var userId = _userService.GetUserId(httpContext);
            return _hubRepository.FindAll(userId);
        }

        public Task<Hub> GetHubById(HttpContext httpContext, Guid id)
        {
            var userId = _userService.GetUserId(httpContext);
            return _hubRepository.Find(userId, id);
        }
    }
}
