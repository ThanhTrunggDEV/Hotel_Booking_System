using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    interface ISignUpViewModel
    {
        string Password { get; set; }
        string PasswordConfirmed { get; set; }
    }
}
