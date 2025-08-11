using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    internal class Invoice
    {
        public string InvoiceID { get; set; } = "";
        public string BookingID { get; set; } = "";
        public double TotalPayment { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
