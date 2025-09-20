using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Amenity
    {
        [Key]
        public string AmenityID { get; set; } = string.Empty;
        public string AmenityName { get; set; } = string.Empty;

        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    }
}
