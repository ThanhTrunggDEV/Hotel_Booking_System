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
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using Microsoft.Win32;

namespace Hotel_Booking_System.ViewModels
{
    public class SuperAdminViewModel : Bindable, ISuperAdminViewModel
    {
        private string _userEmail;
        private IRoomRepository _roomRepository;
        private IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private IHotelRepository _hotelRepository;
        private IUserRepository _userRepository;
        private INavigationService _navigationService;
        private User _currentUser;
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
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<Hotel> Hotels { get; set; }
        public SuperAdminViewModel(IRoomRepository roomRepository, IHotelAdminRequestRepository hotelAdminRequestRepository, IHotelRepository hotelRepository, IUserRepository userRepository, INavigationService navigationService)
        {
            _roomRepository = roomRepository;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _navigationService = navigationService;

            WeakReferenceMessenger.Default.Register<SuperAdminViewModel, MessageService>(this, (recipient, message) =>
             {
                  recipient._userEmail = message.Value;
                 GetCurrentUser();
             });

           
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllAsync();
                TotalHotels = hotels.Count();
                var users = await _userRepository.GetAllAsync();
                TotalUsers = users.Count();
                var requests = _hotelAdminRequestRepository.GetAllAsync().Result.Where(r => r.Status == "Pending").ToList();
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

        private void GetCurrentUser()
        {
            Task.Run(async () =>
            {
                CurrentUser = await _userRepository.GetByEmailAsync(_userEmail);
            }).Wait();
        }
    }
}
