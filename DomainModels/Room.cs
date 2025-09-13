using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Room
    {
        [Key]
        public string RoomID { get; set; }
        public string HotelID { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string RoomImage { get; set; } = "";
        public string RoomType { get; set; } = "";
        public int Capacity { get; set; }
        public double PricePerNight {  get; set; }
        public string Status { get; set; }

    }
}
