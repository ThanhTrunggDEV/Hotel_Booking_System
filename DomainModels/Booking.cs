using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Booking
    {
        [Key]
        public string BookingID { get; set; } = "";
        public string RoomID { get; set; } = "";
        public string UserID { get; set; } = "";
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = "";
    }
}
