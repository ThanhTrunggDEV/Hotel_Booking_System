using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    internal interface IReviewViewModel
    {
        Booking Booking { get; set; }
        int Rating { get; set; }
        string Comment { get; set; }
    }
}
