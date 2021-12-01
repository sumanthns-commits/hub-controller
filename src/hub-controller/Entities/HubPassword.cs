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
    public class HubPassword
    {
        ///<summary>
        /// Map c# types to DynamoDb Columns 
        /// to learn more visit https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/MidLevelAPILimitations.SupportedTypes.html
        /// <summary>
        [DynamoDBHashKey("Id")] //Partition key
        public String UserId { get; set; }

        public static HubPassword Create(string userId, Guid hubId, string passwordHash)
        {
            return new HubPassword()
            {
                UserId = GetPrimaryKey(userId),
                HubId = hubId.ToString(),
                PasswordHash = passwordHash
            };
        }

        public static string GetPrimaryKey(string userId)
        {
            return $"hub_pass_{userId}";
        }

        [DynamoDBRangeKey("SortId")]
        public string HubId { get; set; }

        [DynamoDBProperty]
        public string PasswordHash { get; set; }
    }
}
