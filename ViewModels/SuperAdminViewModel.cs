using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using Microsoft.Win32;

namespace Hotel_Booking_System.ViewModels
{
    public partial class SuperAdminViewModel : Bindable, ISuperAdminViewModel
    {
        private string _userEmail = string.Empty;
        private IRoomRepository _roomRepository;
        private IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private IHotelRepository _hotelRepository;
        private IUserRepository _userRepository;
        private INavigationService _navigationService;
        private User _currentUser = new();
        private int _totalHotels;
        private int _totalUsers;
        private int _pendingRequests;
        public int PendingRequests
        {
            get { return _pendingRequests; }
            set { Set(ref _pendingRequests, value); }
        }
        public int TotalHotels
        {
            get { return _totalHotels; }
            set { Set(ref _totalHotels, value); }
        }
        public int TotalUsers
        {
            get { return _totalUsers; }
            set { Set(ref _totalUsers, value); }
        }
        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                Set(ref _currentUser, value);
            }
        }

        

        public ObservableCollection<HotelAdminRequest> PendingRequest { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Hotel> Hotels { get; set; } = new();
        public ObservableCollection<Hotel> PendingHotels { get; set; } = new();
        public SuperAdminViewModel(IRoomRepository roomRepository, IHotelAdminRequestRepository hotelAdminRequestRepository, IHotelRepository hotelRepository, IUserRepository userRepository, INavigationService navigationService)
        {
            _roomRepository = roomRepository;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _navigationService = navigationService;

            WeakReferenceMessenger.Default.Register<SuperAdminViewModel, MessageService>(this, async (recipient, message) =>
            {
                recipient._userEmail = message.Value;
                await recipient.GetCurrentUserAsync();

            });
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllAsync();
                TotalHotels = hotels.Count(h => h.IsApproved);
                var unapproved = hotels.Where(h => !h.IsApproved).ToList();
                PendingHotels.Clear();
                foreach (var hotel in unapproved)
                {
                    if (!PendingHotels.Any(h => h.HotelID == hotel.HotelID))
                    {
                        PendingHotels.Add(hotel);
                    }
                }
                var users = await _userRepository.GetAllAsync();
                TotalUsers = users.Count();
                var requests = (await _hotelAdminRequestRepository.GetAllAsync()).Where(r => r.Status == "Pending").ToList();
                PendingRequests = requests.Count();

                foreach (var request in requests)
                {
                    if (!PendingRequest.Any(r => r.RequestID == request.RequestID))
                    {
                        PendingRequest.Add(request);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GetCurrentUserAsync()
        {
            var user = await _userRepository.GetByEmailAsync(_userEmail);
            if (user != null)
            {
                CurrentUser = user;
            }
        }

        [RelayCommand]
        private async Task ApproveRequest(string id)
        {
            var request = await _hotelAdminRequestRepository.GetByIdAsync(id);
            if (request == null) return;
            request.Status = "Approved";
            await _hotelAdminRequestRepository.UpdateAsync(request);
            var user = await _userRepository.GetByIdAsync(request.UserID);
            if (user != null)
            {
                user.Role = "HotelAdmin";
                await _userRepository.UpdateAsync(user);
            }
            PendingRequest.Remove(request);
            PendingRequests = PendingRequest.Count;
        }

        [RelayCommand]
        private async Task RejectRequest(string id)
        {
            var request = await _hotelAdminRequestRepository.GetByIdAsync(id);
            if (request == null) return;
            request.Status = "Rejected";
            await _hotelAdminRequestRepository.UpdateAsync(request);
            PendingRequest.Remove(request);
            PendingRequests = PendingRequest.Count;
        }

        [RelayCommand]
        private async Task UpdateInfo()
        {
            await _userRepository.UpdateAsync(CurrentUser);
        }

        [RelayCommand]
        private async Task ApproveHotel(string id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null) return;
            hotel.IsApproved = true;
            await _hotelRepository.UpdateAsync(hotel);
            var pending = PendingHotels.FirstOrDefault(h => h.HotelID == id);
            if (pending != null)
            {
                PendingHotels.Remove(pending);
            }
            TotalHotels++;
        }

        [RelayCommand]
        private async Task RejectHotel(string id)
        {
            await _hotelRepository.DeleteAsync(id);
            await _hotelRepository.SaveAsync();
            var pending = PendingHotels.FirstOrDefault(h => h.HotelID == id);
            if (pending != null)
            {
                PendingHotels.Remove(pending);
            }
        }
    }
}
