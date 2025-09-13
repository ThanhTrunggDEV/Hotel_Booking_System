using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{

       
    interface IHotelAdminViewModel
    {
        ObservableCollection<Hotel> Hotels { get; }
        Hotel? CurrentHotel { get; set; }
        ObservableCollection<Room> Rooms { get; }
        ObservableCollection<Booking> Bookings { get; }
        ObservableCollection<Review> Reviews { get; }
        User CurrentUser { get; set; }
        Task LoadReviewsAsync();
    }
}
