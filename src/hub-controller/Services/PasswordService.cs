using System;
using BCrypt.Net;

namespace HubController.Services
{
    public class PasswordService : IPasswordService
    {
        public string CreateHash(string plainTextPassword)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, salt);
        }

        public bool VerifyPassword(string passwordHash, string providedPlainTextPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPlainTextPassword, passwordHash);
        }
    }
}
