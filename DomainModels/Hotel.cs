using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Hotel
    {
        [Key]
        public string HotelID { get; set; } = null!;
        public string UserID { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string HotelImage { get; set; } = string.Empty;
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVisible { get; set; } = true;

        [NotMapped]
        public double AverageRating { get; set; }

        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    }
}
