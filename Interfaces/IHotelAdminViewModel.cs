using System.Collections.ObjectModel;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface IHotelAdminViewModel
    {
        Hotel? CurrentHotel { get; set; }
        ObservableCollection<Room> Rooms { get; }
        ObservableCollection<Booking> Bookings { get; }
        ObservableCollection<Review> Reviews { get; }
    }
}
