using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Payment
    {
        [Key]
        public string PaymentID { get; set; } = string.Empty;
        public string BookingID { get; set; } = string.Empty;
        public double TotalPayment { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }
}
