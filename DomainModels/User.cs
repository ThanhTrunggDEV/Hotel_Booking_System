using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    internal class User
    {
        public string UserID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
