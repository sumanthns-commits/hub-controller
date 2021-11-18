using HubController.Entities;
using HubController.Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public interface IThingRepository
{
        public Task<Thing> Create(Guid hubId, String name, String description, String thingId);
        public Task<List<Thing>> FindAll(Guid hubId);
    }
}
