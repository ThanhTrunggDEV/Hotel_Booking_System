using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Hotel
    {
        [Key]
        public string HotelID { get; set; }
        public string UserID { get; set; }
        public string HotelName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string HotelImage { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    }
}
