using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class AdminViewModel : Bindable, IAdminViewModel
    {
        private readonly IBookingRepository _bookingRepository;
        public ObservableCollection<Booking> Bookings { get; } = new ObservableCollection<Booking>();

        public AdminViewModel(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
            LoadBookings();
        }

        private void LoadBookings()
        {
            var all = _bookingRepository.GetAllAsync().Result;
            Bookings.Clear();
            foreach (var booking in all)
            {
                Bookings.Add(booking);
            }
        }

        [RelayCommand]
        private async Task CancelBooking(Booking booking)
        {
            if (booking == null)
                return;

            booking.Status = "Cancelled";
            await _bookingRepository.UpdateAsync(booking);
            LoadBookings();
        }

        [RelayCommand]
        private async Task EditBooking(Booking booking)
        {
            if (booking == null)
                return;

            booking.Status = "Modified";
            await _bookingRepository.UpdateAsync(booking);
            LoadBookings();
        }
    }
}
