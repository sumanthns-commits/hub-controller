using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController
{
    public class Constants
    {
        public const String TABLE_NAME = "hub-controller-items";
        public const String DEFAULT_HUBS_ALLOWED_PER_USER = "10";
        public const String DEFAULT_THINGS_ALLOWED_PER_HUB = "10";
        public const int THING_ID_LENGTH = 10; // 8 bit micro-controller cannot handle char array of length > 10
    }
}
