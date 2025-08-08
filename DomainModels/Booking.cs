using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.DomainModels
{
    internal class Booking
    {
        public string BookingID { get; set; } = "";
        public string RoomID { get; set; } = "";
        public string CustomerID { get; set; } = "";
        public string UserID { get; set; } = "";
        public DateTime CheckinDate { get; set; }
        public DateTime CheckoutDate { get; set; }
    }
}
