using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Exceptions
{
    public class LimitExceededException : Exception
    {
        public LimitExceededException(String message): base(message) { }
    }
}
