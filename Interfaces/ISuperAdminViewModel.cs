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

        IAsyncRelayCommand<string> ApproveRequestCommand { get; }
        IAsyncRelayCommand<string> RejectRequestCommand { get; }
        IRelayCommand UpdateInfoCommand { get; }
    }
}

