using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    class Payment
    {
        [Key]
        public string PaymentID { get; set; } = "";
        public string BookingID { get; set; } = "";
        public double TotalPayment { get; set; }
        public string Method { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
