using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class AdminViewModel : Bindable, IAdminViewModel
    {
        private readonly IHotelAdminRequestRepository _requestRepository;
        private readonly IUserRepository _userRepository;

        public ObservableCollection<HotelAdminRequest> Requests { get; set; } = new ObservableCollection<HotelAdminRequest>();

        public AdminViewModel(IHotelAdminRequestRepository requestRepository, IUserRepository userRepository)
        {
            _requestRepository = requestRepository;
            _userRepository = userRepository;
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
