using Amazon.DynamoDBv2.DataModel;
using HubController.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public interface IHubRepository
{
        public Task<Hub> Create(string userId, string name, string description, string password);
        public Task<List<Hub>> FindAll(string userId);
        public Task<Hub> Find(string userId, Guid id);
        public Task<Hub> FindConsistently(string userId, Guid id);
        public Task Delete(string userId, Guid id);
        public Task Save(Hub hub);
    }
}
