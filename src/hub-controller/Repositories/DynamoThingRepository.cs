using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using HubController.Entities;
using HubController.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Repositories
{
    public class DynamoThingRepository : IThingRepository
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;
        private readonly IThingIdGenerator _thingIdGenerator;

        public DynamoThingRepository(IAmazonDynamoDB client, IThingIdGenerator thingIdGenerator)
        {
            _client = client;
            _thingIdGenerator = thingIdGenerator;
            _context = new DynamoDBContext(_client);
        }

        public async Task<Thing> Create(Guid hubId, string name, string description, string thingId)
        {
            var thing = Thing.Create(hubId, name, description, thingId);
            await _context.SaveAsync<Thing>(thing);
            return thing;
        }

        public async Task<List<Thing>> FindAll(Guid hubId)
        {
            var things = new List<Thing>();
            var primaryKey = Thing.GetPrimaryKey(hubId);
            var search = _context.QueryAsync<Thing>(primaryKey);
            do
            {
                things.AddRange(await search.GetNextSetAsync());
            } while (!search.IsDone);

            return things;
        }
    }
}
