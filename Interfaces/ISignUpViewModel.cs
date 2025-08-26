using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    interface ISignUpViewModel
    {
        public string Passoword { get; set; }
        public string PasswordConfirmed { get; set; }
    }
}
