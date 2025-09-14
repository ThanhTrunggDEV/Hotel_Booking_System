using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
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

        IRelayCommand NewHotelCommand { get; }
        IAsyncRelayCommand UpdateHotelInfoCommand { get; }
        IAsyncRelayCommand AddRoomCommand { get; }
        IAsyncRelayCommand<Room?> EditRoomCommand { get; }
        IAsyncRelayCommand<Room?> RemoveRoomCommand { get; }
        IAsyncRelayCommand<Booking?> ConfirmBookingCommand { get; }
        IAsyncRelayCommand<Booking?> CancelBookingCommand { get; }

        Task LoadReviewsAsync();
    }
}
