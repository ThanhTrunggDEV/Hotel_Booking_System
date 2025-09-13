using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Review
    {
        [Key]
        public string ReviewID { get; set; }
        public string UserID { get; set; }
        public string HotelID { get; set; }
        public string RoomID { get; set; }
        public string BookingID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
