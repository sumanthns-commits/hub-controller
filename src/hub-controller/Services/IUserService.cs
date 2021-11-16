using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IUserService
    {
        public String GetUserId(HttpContext httpContext);
    }
}
