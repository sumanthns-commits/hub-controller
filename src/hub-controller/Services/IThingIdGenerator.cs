using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IThingIdGenerator
    {
        public String Generate();
    }
}
