using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using HubController.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public class DynamoHubRespository : IHubRepository
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;

        public DynamoHubRespository(IAmazonDynamoDB client)
        {
            _client = client;
            _context = new DynamoDBContext(_client);
        }


        public async Task<Hub> Create(string userId, string name,
            string description, string passwordHash)
        {
            var hub = Hub.Create(userId, name, description);
            var hubPassword = HubPassword.Create(hub.HubId, passwordHash);
            await _context.SaveAsync<HubPassword>(hubPassword);
            await _context.SaveAsync<Hub>(hub);
            return hub;
        }

        public Task Delete(string userId, Guid id)
        {
            var primaryKey = Hub.GetPrimaryKey(userId);
            return _context.DeleteAsync<Hub>(primaryKey, id);
        }

        public Task<Hub> Find(string userId, Guid id)
        {
            var primaryKey = Hub.GetPrimaryKey(userId);
            return _context.LoadAsync<Hub>(primaryKey, id);
        }

        public async Task<List<Hub>> FindAll(string userId)
        {
            var hubs = new List<Hub>();
            var primaryKey = Hub.GetPrimaryKey(userId);
            var search = _context.QueryAsync<Hub>(primaryKey);
            do
            {
                hubs.AddRange(await search.GetNextSetAsync());
            } while (!search.IsDone);

            return hubs;
        }

        public Task<Hub> FindConsistently(string userId, Guid id)
        {
            var primaryKey = Hub.GetPrimaryKey(userId);
            return _context.LoadAsync<Hub>(primaryKey, id, new DynamoDBOperationConfig()
            {
                ConsistentRead = true
            });
        }

        public Task Save(Hub hub)
        {
            hub.UpdatedAt = DateTime.UtcNow;
            return _context.SaveAsync<Hub>(hub);
        }
    }
}
