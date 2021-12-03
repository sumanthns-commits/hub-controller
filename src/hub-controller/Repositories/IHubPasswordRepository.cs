using HubController.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public interface IHubPasswordRepository
    {
        public Task<HubPassword> Find(string userId, Guid hubId);
    }
}
