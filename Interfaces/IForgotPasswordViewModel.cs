using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    interface IForgotPasswordViewModel
    {
        string StepOneStatus { get; set; }
        string StepTwoStatus { get; set; }
        string StepThreeStatus { get; set; }
        string NewPassword { get; set; }
        string NewPasswordConfirm { get; set; }
    }
}
