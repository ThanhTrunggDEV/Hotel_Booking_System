using Hotel_Booking_System.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Services
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
            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);


                var rfc = new Rfc2898DeriveBytes(password, salt, 100, HashAlgorithmName.SHA256);
                byte[] computedHash = rfc.GetBytes(32);


                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
