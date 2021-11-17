using HubController.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IHubService
    {
        public Task<Hub> CreateHub(HttpContext httpContext, String name);
        public Task<List<Hub>> GetAllHubs(HttpContext httpContext);
        public Task<Hub> GetHubById(HttpContext httpContext, Guid id);
        public Task DeleteHub(HttpContext httpContext, Guid id);
    }
}
