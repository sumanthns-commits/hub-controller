using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Entities
{
    public class UserAuthDetails
    {
        private readonly string _userId;
        private readonly string _hubId;
        private readonly string _password;

        public UserAuthDetails(string userId, string hubId, string password)
        {
            _userId = userId;
            _hubId = hubId;
            _password = password;
        }

        public string UserId { get { return _userId; } }
        public string HubId { get { return _hubId; } }
        public string Password { get { return _password; } }

    }
}
