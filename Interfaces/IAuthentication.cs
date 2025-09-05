using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    public interface IAuthentication
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
