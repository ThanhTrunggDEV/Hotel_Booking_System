using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface ISuperAdminViewModel
    {
        int TotalHotels { get; set; }
        int TotalUsers { get; set; }
        int PendingRequests { get; set; }
        int PendingHotelsCount { get; set; }
        int PendingApprovals { get; set; }
        int ActiveBookings { get; set; }
        double MonthlyRevenue { get; set; }
        ObservableCollection<HotelAdminRequest> PendingRequest { get; set; }
        ObservableCollection<Hotel> PendingHotels { get; set; }
        ObservableCollection<Hotel> FilteredHotels { get; }
        ObservableCollection<User> Users { get; set; }
        ObservableCollection<string> CityOptions { get; }
        string HotelCountText { get; set; }
        Task LoadDataAsync();

    }
}

