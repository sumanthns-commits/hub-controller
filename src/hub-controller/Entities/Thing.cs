using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using HubController.Services;

namespace HubController.Entities
{
    public class Thing
    {
        ///<summary>
        /// Map c# types to DynamoDb Columns 
        /// to learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/MidLevelAPILimitations.SupportedTypes.html
        /// <summary>
        [DynamoDBHashKey("Id")] //Partition key
        public String HubId { get; set; }

        public static Thing Create(string name, string description,
            string thingId)
        {
            return new Thing()
            {
                ThingId = thingId,
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = Constants.THING_OFF
            };
        }

        public static String GetPrimaryKey(Guid hubId)
        {
            return $"hub_thing_{hubId}";
        }

        public String ThingId { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public String Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static List<string> ValidStatuses { get { return new List<string>() { Constants.THING_OFF, Constants.THING_ON }; } }
    }
}
