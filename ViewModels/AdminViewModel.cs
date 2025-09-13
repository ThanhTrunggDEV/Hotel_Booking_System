
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
         private readonly IHotelAdminRequestRepository _requestRepository;
        private readonly IUserRepository _userRepository;
        public ObservableCollection<HotelAdminRequest> Requests { get; set; } = new ObservableCollection<HotelAdminRequest>();

    
        private void LoadBookings()
        {
            var all = _bookingRepository.GetAllAsync().Result;
            Bookings.Clear();
            foreach (var booking in all)
            {
                Bookings.Add(booking);
            }
         }
          

        public AdminViewModel(IHotelAdminRequestRepository requestRepository, IUserRepository userRepository, IBookingRepository bookingRepository)
        {
            _requestRepository = requestRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            LoadBookings();
            LoadRequests();
        }

        private async void LoadRequests()
        {
            var list = await _requestRepository.GetAllAsync();
            Requests.Clear();
            foreach (var r in list)
            {
                Requests.Add(r);
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
        [RelayCommand]
        private async Task ApproveRequest(string id)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return;
            request.Status = "Approved";
            await _requestRepository.UpdateAsync(request);
            var user = await _userRepository.GetByIdAsync(request.UserID);
            if (user != null)
            {
                user.Role = "HotelAdmin";
                await _userRepository.UpdateAsync(user);
            }
            LoadRequests();
        }

        [RelayCommand]
        private async Task RejectRequest(string id)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return;
            request.Status = "Rejected";
            await _requestRepository.UpdateAsync(request);
            LoadRequests();
        }
    }
}
