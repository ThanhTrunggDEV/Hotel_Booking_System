using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface ISuperAdminViewModel
    {
        int TotalHotels { get; set; }
        int TotalUsers { get; set; }
        int PendingRequests { get; set; }
        ObservableCollection<HotelAdminRequest> PendingRequest { get; set; }
        ObservableCollection<Hotel> PendingHotels { get; set; }
        Task LoadDataAsync();

    }
}

