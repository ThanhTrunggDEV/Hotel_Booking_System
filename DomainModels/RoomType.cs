using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    internal class RoomType
    {
        public string RoomTypeID { get; set; } = "";
        public string Type { get; set; } = "";
        public double PricePerNight { get; set; }
    }
}
