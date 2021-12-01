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
    public class HubService : IHubService
    {
        private readonly IHubRepository _hubRepository;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;

        public HubService(IHubRepository hubRepoitory, IUserService userService, IPasswordService passwordService)
        {
            _hubRepository = hubRepoitory;
            _userService = userService;
            _passwordService = passwordService;
        }

        public async Task<Hub> CreateHub(HttpContext httpContext, HubDAO hubDAO)
        {
            var allowedNumberOfHubsPerUser = Int32.Parse(Environment.GetEnvironmentVariable("HUBS_ALLOWED_PER_USER") ?? Constants.DEFAULT_HUBS_ALLOWED_PER_USER);
            var userId = _userService.GetUserId(httpContext);
            var hubs = await _hubRepository.FindAll(userId);
            
            // Hubs are idempotent by name
            var existingHub = hubs.FirstOrDefault(h => h.Name.Equals(hubDAO.Name));
            if (existingHub != null)
            {
                return existingHub;
            }
            
            // Check if user has already exceeded limit
            if (hubs.Count >= allowedNumberOfHubsPerUser)
            {
                throw new LimitExceededException("Cannot create more hubs. Limit reached.");
            }
            var passwordHash = _passwordService.CreateHash(hubDAO.Password);
            return await _hubRepository.Create(userId, hubDAO.Name, hubDAO.Description, passwordHash);
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

        public Task SaveHub(Hub hub)
        {
            return _hubRepository.Save(hub);
        }
    }
}
