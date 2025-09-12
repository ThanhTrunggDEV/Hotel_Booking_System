using System.Collections.ObjectModel;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface IAdminViewModel
    {
        ObservableCollection<HotelAdminRequest> Requests { get; set; }
    }
}
