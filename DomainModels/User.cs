using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class User
    {
        
        [Key]
        public string UserID { get; set; }
        public string FullName { get; set; } = "";
        public string AvatarUrl { get; set; } = "https://i.ibb.co/FLvg0hX4/avatar-default.png";
        public string Phone { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
