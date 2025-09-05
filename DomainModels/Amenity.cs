using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    internal class Amenity
    {
        [Key]
        public string AmenityID { get; set; }
        public string AmenityName { get; set; } = "";
    }
}
