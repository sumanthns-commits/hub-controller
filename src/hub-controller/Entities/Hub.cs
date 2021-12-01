using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;


namespace HubController.Entities
{
    /// <summary>
    /// Map the Book Class to DynamoDb Table
    /// To learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DeclarativeTagsList.html
    /// </summary>
    
    [DynamoDBTable(Constants.TABLE_NAME)]
    public class Hub
    {
        ///<summary>
        /// Map c# types to DynamoDb Columns 
        /// to learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/MidLevelAPILimitations.SupportedTypes.html
        /// <summary>
        [DynamoDBHashKey("Id")] //Partition key
        public String UserId { get; set; }

        public static Hub Create(string userId, string name, string description)
        {
            return new Hub()
            {
                UserId = GetPrimaryKey(userId),
                HubId = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static String GetPrimaryKey(string userId)
        {
            return $"user_hub_{userId}";
        }

        [DynamoDBRangeKey("SortId")]
        public Guid HubId { get; set; }

        [DynamoDBProperty]
        public String Name { get; set; }

        [DynamoDBProperty]
        public String Description { get; set; }

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty]
        public DateTime UpdatedAt { get; set; }

        [DynamoDBProperty]
        public List<Thing> Things { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }
    }
}
