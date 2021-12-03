using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using HubController.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public class DynamoHubPasswordRepository : IHubPasswordRepository
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;

        public DynamoHubPasswordRepository(IAmazonDynamoDB client)
        {
            _client = client;
            _context = new DynamoDBContext(_client);
        }

        public Task<HubPassword> Find(string userId, Guid hubId)
        {
            var primaryKey = HubPassword.GetPrimaryKey(userId);
            return _context.LoadAsync<HubPassword>(primaryKey, hubId.ToString());
        }
    }
}
