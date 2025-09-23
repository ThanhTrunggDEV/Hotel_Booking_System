using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAuthentication _authenticationService;
        private User _currentUser = new();
        private int _totalHotels;
        private int _totalUsers;
        private int _pendingRequests;
        private int _pendingHotelsCount;
        private int _pendingApprovals;
        private int _activeBookings;
        private double _monthlyRevenue;
        private int _activeHotelsCount;
        private int _suspendedHotelsCount;
        private double _averageHotelRating;
        private string _selectedCity = "All Cities";
        private string _selectedHotelStatus = "All";
        private string _hotelSearchTerm = string.Empty;
        private string _hotelCountText = string.Empty;
        private int _totalBookings;
        private double _totalSpent;
        private string _membershipLevel = "Bronze";
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
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

        public int TotalBookings
        {
            get => _totalBookings;
            set => Set(ref _totalBookings, value);
        }

        public double TotalSpent
        {
            get => _totalSpent;
            set => Set(ref _totalSpent, value);
        }

        public string MembershipLevel
        {
            get => _membershipLevel;
            set => Set(ref _membershipLevel, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set => Set(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => Set(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => Set(ref _confirmPassword, value);
        }



        public ObservableCollection<HotelAdminRequest> PendingRequest { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Hotel> Hotels { get; set; } = new();
        public ObservableCollection<Hotel> PendingHotels { get; set; } = new();
        public ObservableCollection<Hotel> FilteredHotels { get; } = new();
        public ObservableCollection<string> CityOptions { get; } = new();
        public SuperAdminViewModel(IRoomRepository roomRepository, IHotelAdminRequestRepository hotelAdminRequestRepository, IHotelRepository hotelRepository, IUserRepository userRepository, INavigationService navigationService, IBookingRepository bookingRepository, IPaymentRepository paymentRepository, IAuthentication authenticationService)
        {
            _roomRepository = roomRepository;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _navigationService = navigationService;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _authenticationService = authenticationService;

            CityOptions.Add("All Cities");

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
                if (string.IsNullOrWhiteSpace(CurrentUser?.UserID))
                {
                    await GetCurrentUserAsync();
                }

                var hotels = await _hotelRepository.GetAllAsync();
                var rooms = await _roomRepository.GetAllAsync();
                var users = await _userRepository.GetAllAsync();

                TotalHotels = hotels.Count(h => h.IsApproved);
                TotalUsers = users.Count();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                var userLookup = users.ToDictionary(u => u.UserID, u => u.FullName);
                var roomGroups = rooms.GroupBy(r => r.HotelID)
                    .ToDictionary(g => g.Key, g => g.Count());

                Hotels.Clear();
                PendingHotels.Clear();

                foreach (var hotel in hotels)
                {
                    if (userLookup.TryGetValue(hotel.UserID, out var adminName))
                    {
                        hotel.AdminName = adminName;
                    }

                    hotel.TotalRooms = roomGroups.TryGetValue(hotel.HotelID, out var count) ? count : 0;
                    hotel.Status = GetHotelStatus(hotel);
                    hotel.CreatedDate = null;

                    Hotels.Add(hotel);

                    if (!hotel.IsApproved && !PendingHotels.Any(h => h.HotelID == hotel.HotelID))
                    {
                        PendingHotels.Add(hotel);
                    }
                }

                UpdateCityOptions();
                UpdateHotelStats();
                ApplyHotelFilters();

                var bookings = await _bookingRepository.GetAllAsync();
                ActiveBookings = bookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) || string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
                var payments = await _paymentRepository.GetAllAsync();
                var today = DateTime.Today;
                MonthlyRevenue = payments
                    .Where(p => p.PaymentDate.Year == today.Year && p.PaymentDate.Month == today.Month)
                    .Sum(p => p.TotalPayment);

                UpdateUserBookingStats(bookings, payments);

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

        public int ActiveHotelsCount
        {
            get => _activeHotelsCount;
            set => Set(ref _activeHotelsCount, value);
        }

        public int SuspendedHotelsCount
        {
            get => _suspendedHotelsCount;
            set => Set(ref _suspendedHotelsCount, value);
        }

        public double AverageHotelRating
        {
            get => _averageHotelRating;
            set => Set(ref _averageHotelRating, value);
        }

        public string SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (_selectedCity != value)
                {
                    Set(ref _selectedCity, value);
                    ApplyHotelFilters();
                }
            }
        }

        public string SelectedHotelStatus
        {
            get => _selectedHotelStatus;
            set
            {
                if (_selectedHotelStatus != value)
                {
                    Set(ref _selectedHotelStatus, value);
                    ApplyHotelFilters();
                }
            }
        }

        public string HotelSearchTerm
        {
            get => _hotelSearchTerm;
            set
            {
                if (_hotelSearchTerm != value)
                {
                    Set(ref _hotelSearchTerm, value);
                    ApplyHotelFilters();
                }
            }
        }

        public string HotelCountText
        {
            get => _hotelCountText;
            set => Set(ref _hotelCountText, value);
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
        private async Task UploadImage()
        {
            FileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
                {
                    MessageBox.Show("Current user information is not available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    CurrentUser.AvatarUrl = await UploadImageService.UploadAsync(openFileDialog.FileName);
                    await _userRepository.UpdateAsync(CurrentUser);
                    var refreshed = await _userRepository.GetByIdAsync(CurrentUser.UserID);
                    if (refreshed != null)
                    {
                        CurrentUser = refreshed;
                    }

                    MessageBox.Show("Profile image updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task UpdateInfo()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                MessageBox.Show("Current user information is not available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _userRepository.UpdateAsync(CurrentUser);
                var refreshed = await _userRepository.GetByIdAsync(CurrentUser.UserID);
                if (refreshed != null)
                {
                    CurrentUser = refreshed;
                }

                MessageBox.Show("Profile updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                MessageBox.Show("Current user information is not available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_authenticationService.VerifyPassword(CurrentPassword, CurrentUser.Password))
            {
                MessageBox.Show("Current password is incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsPasswordValid(NewPassword))
            {
                MessageBox.Show("New password does not meet the security requirements.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("New password and confirmation do not match.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CurrentUser.Password = _authenticationService.HashPassword(NewPassword);
                await _userRepository.UpdateAsync(CurrentUser);
                var refreshed = await _userRepository.GetByIdAsync(CurrentUser.UserID);
                if (refreshed != null)
                {
                    CurrentUser = refreshed;
                }

                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearPasswordFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearPasswordFields()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
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
            var localHotel = Hotels.FirstOrDefault(h => h.HotelID == id);
            if (localHotel != null)
            {
                localHotel.IsApproved = true;
                localHotel.Status = GetHotelStatus(localHotel);
            }
            TotalHotels++;
            UpdateHotelStats();
            UpdateCityOptions();
            ApplyHotelFilters();
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
            var localHotel = Hotels.FirstOrDefault(h => h.HotelID == id);
            if (localHotel != null)
            {
                Hotels.Remove(localHotel);
            }
            var filteredHotel = FilteredHotels.FirstOrDefault(h => h.HotelID == id);
            if (filteredHotel != null)
            {
                FilteredHotels.Remove(filteredHotel);
            }
            UpdateHotelStats();
            UpdateCityOptions();
            ApplyHotelFilters();
            UpdatePendingCounts();
        }

        private void UpdatePendingCounts()
        {
            PendingHotelsCount = PendingHotels.Count;
            PendingRequests = PendingRequest.Count;
            PendingApprovals = PendingRequests + PendingHotelsCount;
        }

        private void UpdateUserBookingStats(IEnumerable<Booking> bookings, IEnumerable<Payment> payments)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                TotalBookings = 0;
                TotalSpent = 0;
                MembershipLevel = "Bronze";
                return;
            }

            var userBookings = bookings.Where(b => b.UserID == CurrentUser.UserID).ToList();
            TotalBookings = userBookings.Count;

            if (userBookings.Count == 0)
            {
                TotalSpent = 0;
                MembershipLevel = "Bronze";
                return;
            }

            var bookingIds = new HashSet<string>(userBookings.Select(b => b.BookingID));
            var totalSpent = payments.Where(p => bookingIds.Contains(p.BookingID)).Sum(p => p.TotalPayment);

            TotalSpent = totalSpent;
            MembershipLevel = GetMembershipLevel(totalSpent);
        }

        private string GetHotelStatus(Hotel hotel)
        {
            if (!hotel.IsApproved)
            {
                return "Pending";
            }

            return hotel.IsVisible ? "Active" : "Suspended";
        }

        private void UpdateCityOptions()
        {
            var currentSelection = SelectedCity;
            var cities = Hotels
                .Select(h => h.City)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            CityOptions.Clear();
            CityOptions.Add("All Cities");
            foreach (var city in cities)
            {
                CityOptions.Add(city);
            }

            var matchingCity = CityOptions.FirstOrDefault(c => string.Equals(c, currentSelection, StringComparison.OrdinalIgnoreCase));

            SelectedCity = matchingCity ?? "All Cities";
        }

        private void ApplyHotelFilters()
        {
            if (Hotels == null)
            {
                return;
            }

            IEnumerable<Hotel> filtered = Hotels;

            if (!string.IsNullOrWhiteSpace(SelectedCity) && SelectedCity != "All Cities")
            {
                filtered = filtered.Where(h => string.Equals(h.City, SelectedCity, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedHotelStatus) && SelectedHotelStatus != "All")
            {
                filtered = filtered.Where(h => string.Equals(h.Status, SelectedHotelStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(HotelSearchTerm))
            {
                filtered = filtered.Where(h =>
                    (!string.IsNullOrWhiteSpace(h.HotelName) && h.HotelName.Contains(HotelSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(h.Address) && h.Address.Contains(HotelSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(h.City) && h.City.Contains(HotelSearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredHotels.Clear();
            foreach (var hotel in filtered.OrderBy(h => h.HotelName))
            {
                FilteredHotels.Add(hotel);
            }

            HotelCountText = FilteredHotels.Count == Hotels.Count
                ? $"Showing {FilteredHotels.Count} of {Hotels.Count} hotels"
                : $"Showing {FilteredHotels.Count} of {Hotels.Count} hotels (filtered)";
        }

        private void UpdateHotelStats()
        {
            ActiveHotelsCount = Hotels.Count(h => h.IsApproved && h.IsVisible);
            SuspendedHotelsCount = Hotels.Count(h => h.IsApproved && !h.IsVisible);
            PendingHotelsCount = Hotels.Count(h => !h.IsApproved);
            AverageHotelRating = Hotels.Count > 0 ? Math.Round(Hotels.Average(h => h.Rating), 1) : 0;
        }

        [RelayCommand]
        private void SearchHotels()
        {
            ApplyHotelFilters();
        }

        [RelayCommand]
        private void ClearHotelSearch()
        {
            HotelSearchTerm = string.Empty;
        }

        [RelayCommand]
        private void SetHotelStatusFilter(string status)
        {
            SelectedHotelStatus = status;
        }

        [RelayCommand]
        private async Task RefreshHotelsAsync()
        {
            await LoadDataAsync();
        }

        private static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetMembershipLevel(double totalSpent)
        {
            if (totalSpent >= 10000) return "Platinum";
            if (totalSpent >= 5000) return "Gold";
            if (totalSpent >= 1000) return "Silver";
            return "Bronze";
        }
    }
}
