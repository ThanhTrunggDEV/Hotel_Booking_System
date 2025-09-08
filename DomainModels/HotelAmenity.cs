using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class HotelAmenity
    {
        [Key]
        public string AmenityID { get; set; }
        public string HotelID { get; set; } = "";
    }
}
