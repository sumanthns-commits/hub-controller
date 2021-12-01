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
        public String HubId { get; set; }

        public static HubPassword Create(Guid hubId, string passwordHash)
        {
            return new HubPassword()
            {
                HubId = GetPrimaryKey(hubId),
                PasswordHash = passwordHash
            };
        }

        public static String GetPrimaryKey(Guid hubId)
        {
            return $"hub_pass_{hubId}";
        }

        [DynamoDBProperty]
        public string PasswordHash { get; set; }
    }
}
