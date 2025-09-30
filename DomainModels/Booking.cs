using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Booking
    {
        [Key]
        public string BookingID { get; set; } = null!;
        public string HotelID { get; set; } = string.Empty;
        public string RoomID { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;

        [NotMapped]
        public string RoomNumber { get; set; } = string.Empty;

        [NotMapped]
        public Review? UserReview { get; set; }

        [NotMapped]
        public int ReviewRating { get; set; }

        [NotMapped]
        public string ReviewComment { get; set; } = string.Empty;

        [NotMapped]
        public bool HasReview => UserReview != null;

        [NotMapped]
        public string ReviewStars => ReviewRating > 0 ? new string('★', ReviewRating) : string.Empty;
    }
}
