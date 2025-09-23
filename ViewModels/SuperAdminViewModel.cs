using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class SuperAdminViewModel : Bindable, ISuperAdminViewModel
    {
        private string _userEmail = string.Empty;
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private User _currentUser = new();
        private int _totalHotels;
        private int _totalUsers;
        private int _pendingRequests;
        private int _pendingHotelsCount;
        private int _pendingApprovals;
        private int _activeBookings;
        private double _monthlyRevenue;
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
        public int ActiveBookings
        {
            get { return _activeBookings; }
            set { Set(ref _activeBookings, value); }
        }
        public double MonthlyRevenue
        {
            get { return _monthlyRevenue; }
            set { Set(ref _monthlyRevenue, value); }
        }
        public int PendingHotelsCount
        {
            get { return _pendingHotelsCount; }
            set { Set(ref _pendingHotelsCount, value); }
        }
        public int PendingApprovals
        {
            get { return _pendingApprovals; }
            set { Set(ref _pendingApprovals, value); }
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
        public SuperAdminViewModel(IRoomRepository roomRepository, IHotelAdminRequestRepository hotelAdminRequestRepository, IHotelRepository hotelRepository, IUserRepository userRepository, INavigationService navigationService, IBookingRepository bookingRepository, IPaymentRepository paymentRepository)
        {
            _roomRepository = roomRepository;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _navigationService = navigationService;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;

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
                var userLookup = users.ToDictionary(u => u.UserID, u => u.FullName);
                var bookings = await _bookingRepository.GetAllAsync();
                ActiveBookings = bookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) || string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
                var payments = await _paymentRepository.GetAllAsync();
                var today = DateTime.Today;
                MonthlyRevenue = payments
                    .Where(p => p.PaymentDate.Year == today.Year && p.PaymentDate.Month == today.Month)
                    .Sum(p => p.TotalPayment);

                var requests = (await _hotelAdminRequestRepository.GetAllAsync()).Where(r => r.Status == "Pending").ToList();
                PendingRequest.Clear();

                foreach (var request in requests)
                {
                    if (userLookup.TryGetValue(request.UserID, out var applicantName))
                    {
                        request.ApplicantName = applicantName;
                    }

                    if (!PendingRequest.Any(r => r.RequestID == request.RequestID))
                    {
                        PendingRequest.Add(request);
                    }
                }

                UpdatePendingCounts();

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
            var pending = PendingRequest.FirstOrDefault(r => r.RequestID == id);
            if (pending != null)
            {
                PendingRequest.Remove(pending);
            }
            UpdatePendingCounts();
        }

        [RelayCommand]
        private async Task RejectRequest(string id)
        {
            var request = await _hotelAdminRequestRepository.GetByIdAsync(id);
            if (request == null) return;
            request.Status = "Rejected";
            await _hotelAdminRequestRepository.UpdateAsync(request);
            var pending = PendingRequest.FirstOrDefault(r => r.RequestID == id);
            if (pending != null)
            {
                PendingRequest.Remove(pending);
            }
            UpdatePendingCounts();
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
            UpdatePendingCounts();
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
            UpdatePendingCounts();
        }

        private void UpdatePendingCounts()
        {
            PendingHotelsCount = PendingHotels.Count;
            PendingRequests = PendingRequest.Count;
            PendingApprovals = PendingRequests + PendingHotelsCount;
        }
    }
}
