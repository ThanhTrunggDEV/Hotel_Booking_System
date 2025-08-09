using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Interfaces
{
    internal interface IAuthentication
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
