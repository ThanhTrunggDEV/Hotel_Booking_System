using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{

       
    public interface IHotelAdminViewModel
    {
        ObservableCollection<Hotel> Hotels { get; }
        Hotel? CurrentHotel { get; set; }
        ObservableCollection<Room> Rooms { get; }
        ObservableCollection<Booking> Bookings { get; }
        ObservableCollection<Review> Reviews { get; }
        User CurrentUser { get; set; }

        bool HasFreeWifi { get; set; }
        bool HasSwimmingPool { get; set; }
        bool HasFreeParking { get; set; }
        bool HasRestaurant { get; set; }
        bool HasGym { get; set; }

        int TotalReviews { get; }
        double AverageRating { get; }

        int FiveStarCount { get; }
        int FourStarCount { get; }
        int ThreeStarCount { get; }
        int TwoStarCount { get; }
        int OneStarCount { get; }

        double FiveStarRatio { get; }
        double FourStarRatio { get; }
        double ThreeStarRatio { get; }
        double TwoStarRatio { get; }
        double OneStarRatio { get; }

        Task LoadReviewsAsync();
    }
}
