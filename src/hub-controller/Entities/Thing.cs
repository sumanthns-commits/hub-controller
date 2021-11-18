using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using HubController.Services;

namespace HubController.Entities
{
    /// <summary>
    /// Map the Book Class to DynamoDb Table
    /// To learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DeclarativeTagsList.html
    /// </summary>

    [DynamoDBTable(Constants.TABLE_NAME)]
    public class Thing
    {
        private const String OFF = "off";
        private const String ON = "on";
        ///<summary>
        /// Map c# types to DynamoDb Columns 
        /// to learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/MidLevelAPILimitations.SupportedTypes.html
        /// <summary>
        [DynamoDBHashKey("Id")] //Partition key
        public String HubId { get; set; }

        public static Thing Create(Guid hubId, string name,
            string description, string thingId)
        {
            return new Thing()
            {
                HubId = GetPrimaryKey(hubId),
                ThingId = thingId,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                Status = OFF
            };
        }

        public static String GetPrimaryKey(Guid hubId)
        {
            return $"hub_thing_{hubId}";
        }

        [DynamoDBRangeKey("SortId")]
        public String ThingId { get; set; }

        [DynamoDBProperty]
        public String Name { get; set; }

        [DynamoDBProperty]
        public String Description { get; set; }

        [DynamoDBProperty]
        public String Status { get; set; }

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }
    }
}
