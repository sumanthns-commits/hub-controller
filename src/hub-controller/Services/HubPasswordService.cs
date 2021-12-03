using HubController.Entities;
using HubController.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public class HubPasswordService : IHubPasswordService
    {
        private readonly IHubPasswordRepository _hubPasswordRepository;
        private readonly IPasswordService _passwordService;

        public HubPasswordService(IHubPasswordRepository hubPasswordRepository, IPasswordService passwordService)
        {
            _hubPasswordRepository = hubPasswordRepository;
            _passwordService = passwordService;
        }

        public async Task<bool> VerifyPassword(string userId, Guid hubId, string plainTextPassword)
        {
            var hubPassword = await _hubPasswordRepository.Find(userId, hubId);
            if(hubPassword == null)
            {
                return false;
            }
            return _passwordService.VerifyPassword(hubPassword.PasswordHash, plainTextPassword);

        }
    }
}
