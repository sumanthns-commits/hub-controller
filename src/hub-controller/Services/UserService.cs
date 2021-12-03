using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public class UserService : IUserService
    {
        public String GetUserId(HttpContext httpContext)
        {
            var subjectClaim = httpContext.User.Claims.First(claim => claim.Type == Constants.USER_SUBJECT_CLAIM_TYPE);
            if (subjectClaim == null)
            {
                throw new Exception("Subject claim is not found on user");
            }
            return subjectClaim.Value;
        }
    }
}
