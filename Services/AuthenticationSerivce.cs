using Hotel_Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Services
{
    internal class AuthenticationSerivce : IAuthentication
    {
        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var rfc = new Rfc2898DeriveBytes(password, salt,100,HashAlgorithmName.SHA256);
            
            byte[] hash = rfc.GetBytes(32);

            string hashedPassword = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            return hashedPassword;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
