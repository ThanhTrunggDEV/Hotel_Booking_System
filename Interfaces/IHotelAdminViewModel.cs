using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
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

        ICommand NewHotelCommand { get; }
        ICommand UpdateHotelInfoCommand { get; }
        ICommand AddRoomCommand { get; }
        ICommand EditRoomCommand { get; }
        ICommand RemoveRoomCommand { get; }
        ICommand ConfirmBookingCommand { get; }
        ICommand CancelBookingCommand { get; }

        Task LoadReviewsAsync();
    }
}
