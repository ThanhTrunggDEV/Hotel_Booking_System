using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    class Payment
    {
        public string PaymentID { get; set; } = "";
        public string BookingID { get; set; } = "";
        public double TotalPayment { get; set; }
        public string Method { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
