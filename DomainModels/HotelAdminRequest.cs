using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class HotelAdminRequest
    {
        [Key]
        public string RequestID { get; set; }
        public string UserID { get; set; }
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
