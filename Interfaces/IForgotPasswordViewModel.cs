using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    interface IForgotPasswordViewModel
    {
        public string StepOneStatus { get; set; }
        public string StepTwoStatus { get; set; }
        public string StepThreeStatus { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
    }
}
