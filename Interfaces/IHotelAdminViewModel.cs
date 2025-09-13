using System.Collections.ObjectModel;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface IHotelAdminViewModel
    {
        ObservableCollection<Room> Rooms { get; }
        ObservableCollection<Review> Reviews { get; }
    }
}
