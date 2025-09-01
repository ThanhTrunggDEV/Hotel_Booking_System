using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hotel_Booking_System.Interfaces
{
    interface ILoginViewModel
    {
        public string Password { get; set; }
        public string Email { get; set; }   
        public bool IsSavedCredentials { get; set; }
    }
}
