using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HubController.Services
{
    public interface IPasswordService
    {
        public string CreateHash(string plainTextPassword);
        public bool VerifyPassword(string passwordHash, string providedPlainTextPassword);
    }
}
