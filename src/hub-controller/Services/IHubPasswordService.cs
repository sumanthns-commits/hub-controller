using HubController.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IHubPasswordService
    {
        public Task<bool> VerifyPassword(string userId, Guid hubId, string plainTextPassword);
    }
}
