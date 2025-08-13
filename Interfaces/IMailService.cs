using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    internal interface IMailService
    {
        void SendOTP(string otp, string receiver);

    }
}
