using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.Views;
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
        private List<Room> _allRooms = new();
        private List<Booking> _allBookings = new();
        private List<Payment> _allPayments = new();
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
        private string _notificationMessage = string.Empty;
        private HotelAdminRequest? _selectedPendingRequest;
        private Hotel? _selectedPendingHotel;
        private Hotel? _selectedHotel;
        private User? _selectedUser;
        private int _customerCount;
        private int _hotelAdminCount;
        private int _superAdminCount;
        private string _selectedUserRole = "All";
        private string _userSearchTerm = string.Empty;
        private string _userFilterSummary = string.Empty;
        private DispatcherTimer? _notificationTimer;
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

        public int CustomerCount
        {
            get => _customerCount;
            set => Set(ref _customerCount, value);
        }

        public int HotelAdminCount
        {
            get => _hotelAdminCount;
            set => Set(ref _hotelAdminCount, value);
        }

        public int SuperAdminCount
        {
            get => _superAdminCount;
            set => Set(ref _superAdminCount, value);
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

        public string NotificationMessage
        {
            get => _notificationMessage;
            set => Set(ref _notificationMessage, value);
        }

        public HotelAdminRequest? SelectedPendingRequest
        {
            get => _selectedPendingRequest;
            set => Set(ref _selectedPendingRequest, value);
        }

        public Hotel? SelectedPendingHotel
        {
            get => _selectedPendingHotel;
            set => Set(ref _selectedPendingHotel, value);
        }

        public Hotel? SelectedHotel
        {
            get => _selectedHotel;
            set => Set(ref _selectedHotel, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => Set(ref _selectedUser, value);
        }

        public string SelectedUserRole
        {
            get => _selectedUserRole;
            set
            {
                if (_selectedUserRole != value)
                {
                    Set(ref _selectedUserRole, value);
                    ApplyUserFilters();
                }
            }
        }

        public string UserSearchTerm
        {
            get => _userSearchTerm;
            set
            {
                if (_userSearchTerm != value)
                {
                    Set(ref _userSearchTerm, value);
                    ApplyUserFilters();
                }
            }
        }

        public string UserFilterSummary
        {
            get => _userFilterSummary;
            set => Set(ref _userFilterSummary, value);
        }



        public ObservableCollection<HotelAdminRequest> PendingRequest { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Hotel> Hotels { get; set; } = new();
        public ObservableCollection<Hotel> PendingHotels { get; set; } = new();
        public ObservableCollection<Hotel> FilteredHotels { get; } = new();
        public ObservableCollection<User> FilteredUsers { get; } = new();
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
                var previousRequestId = SelectedPendingRequest?.RequestID;
                var previousHotelId = SelectedPendingHotel?.HotelID;

                if (string.IsNullOrWhiteSpace(CurrentUser?.UserID))
                {
                    await GetCurrentUserAsync();
                }

                var hotels = await _hotelRepository.GetAllAsync();
                var rooms = await _roomRepository.GetAllAsync();
                var users = await _userRepository.GetAllAsync();

                _allRooms = rooms;
                _allBookings = await _bookingRepository.GetAllAsync();
                _allPayments = await _paymentRepository.GetAllAsync();

                TotalHotels = hotels.Count(h => h.IsApproved);
                TotalUsers = users.Count();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                UpdateUserRoleCounts();
                ApplyUserFilters();

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

                SelectedPendingHotel = PendingHotels.FirstOrDefault(h => h.HotelID == previousHotelId) ?? PendingHotels.FirstOrDefault();

                UpdateCityOptions();
                UpdateHotelStats();
                ApplyHotelFilters();
                SelectedHotel ??= FilteredHotels.FirstOrDefault();

                ActiveBookings = _allBookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) || string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
                var today = DateTime.Today;
                MonthlyRevenue = _allPayments
                    .Where(p => p.PaymentDate.Year == today.Year && p.PaymentDate.Month == today.Month)
                    .Sum(p => p.TotalPayment);

                UpdateUserBookingStats(_allBookings, _allPayments);

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

                SelectedPendingRequest = PendingRequest.FirstOrDefault(r => r.RequestID == previousRequestId) ?? PendingRequest.FirstOrDefault();

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
            if (SelectedPendingRequest == null || SelectedPendingRequest.RequestID == id)
            {
                SelectedPendingRequest = PendingRequest.FirstOrDefault();
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
            if (SelectedPendingRequest == null || SelectedPendingRequest.RequestID == id)
            {
                SelectedPendingRequest = PendingRequest.FirstOrDefault();
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
                    ShowNotification("Current user information is not available.");
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

                    ShowNotification("Profile image updated successfully.");
                }
                catch (Exception ex)
                {
                    ShowNotification($"Failed to upload image: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task UpdateInfo()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                ShowNotification("Current user information is not available.");
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

                ShowNotification("Profile updated successfully.");
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to update profile: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                ShowNotification("Current user information is not available.");
                return;
            }

            if (!_authenticationService.VerifyPassword(CurrentPassword, CurrentUser.Password))
            {
                ShowNotification("Current password is incorrect.");
                return;
            }

            if (!IsPasswordValid(NewPassword))
            {
                ShowNotification("New password does not meet the security requirements.");
                return;
            }

            if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                ShowNotification("New password and confirmation do not match.");
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

                ShowNotification("Password changed successfully.");
                ClearPasswordFields();
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to change password: {ex.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            NotificationMessage = message;
            _notificationTimer?.Stop();
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _notificationTimer.Tick += (s, e) =>
            {
                NotificationMessage = string.Empty;
                _notificationTimer?.Stop();
            };
            _notificationTimer.Start();
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
            if (SelectedPendingHotel == null || SelectedPendingHotel.HotelID == id)
            {
                SelectedPendingHotel = PendingHotels.FirstOrDefault();
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
            if (SelectedPendingHotel == null || SelectedPendingHotel.HotelID == id)
            {
                SelectedPendingHotel = PendingHotels.FirstOrDefault();
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

            var previousSelection = SelectedHotel?.HotelID;
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

            SelectedHotel = FilteredHotels.FirstOrDefault(h => h.HotelID == previousSelection) ?? FilteredHotels.FirstOrDefault();
        }

        private void UpdateHotelStats()
        {
            ActiveHotelsCount = Hotels.Count(h => h.IsApproved && h.IsVisible);
            SuspendedHotelsCount = Hotels.Count(h => h.IsApproved && !h.IsVisible);
            PendingHotelsCount = Hotels.Count(h => !h.IsApproved);
            AverageHotelRating = Hotels.Count > 0 ? Math.Round(Hotels.Average(h => h.Rating), 1) : 0;
        }

        private void UpdateUserRoleCounts()
        {
            CustomerCount = Users.Count(u => NormalizeRole(u.Role) == "customer");
            HotelAdminCount = Users.Count(u => NormalizeRole(u.Role) == "hoteladmin");
            SuperAdminCount = Users.Count(u =>
            {
                var role = NormalizeRole(u.Role);
                return role == "superadmin" || role == "admin";
            });
        }

        private void ApplyUserFilters()
        {
            if (Users == null)
            {
                return;
            }

            var previousSelection = SelectedUser?.UserID;
            IEnumerable<User> filtered = Users;

            if (!string.IsNullOrWhiteSpace(SelectedUserRole) && !string.Equals(SelectedUserRole, "All", StringComparison.OrdinalIgnoreCase))
            {
                var normalizedRole = NormalizeRole(SelectedUserRole);
                filtered = filtered.Where(u => string.Equals(NormalizeRole(u.Role), normalizedRole, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(UserSearchTerm))
            {
                filtered = filtered.Where(u =>
                    (!string.IsNullOrWhiteSpace(u.FullName) && u.FullName.Contains(UserSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.Email) && u.Email.Contains(UserSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(u.Phone) && u.Phone.Contains(UserSearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredUsers.Clear();
            foreach (var user in filtered.OrderBy(u => u.FullName ?? u.Email ?? u.UserID))
            {
                FilteredUsers.Add(user);
            }

            UserFilterSummary = FilteredUsers.Count == Users.Count
                ? $"Showing {FilteredUsers.Count} of {Users.Count} users"
                : $"Showing {FilteredUsers.Count} of {Users.Count} users (filtered)";

            SelectedUser = FilteredUsers.FirstOrDefault(u => u.UserID == previousSelection) ?? FilteredUsers.FirstOrDefault();
        }

        private static string NormalizeRole(string? role)
        {
            return string.IsNullOrWhiteSpace(role)
                ? string.Empty
                : role.Trim().Replace(" ", string.Empty).ToLowerInvariant();
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
        private void SearchUsers()
        {
            ApplyUserFilters();
        }

        [RelayCommand]
        private void ClearUserSearch()
        {
            UserSearchTerm = string.Empty;
        }

        [RelayCommand]
        private void SetUserRoleFilter(string role)
        {
            SelectedUserRole = role;
        }

        [RelayCommand]
        private async Task RefreshHotelsAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private void Logout()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            ClearPasswordFields();
            _navigationService.NavigateToLogin();
        }

        [RelayCommand]
        private void ViewHotelDetails(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Hotel ID: {hotel.HotelID}");
            builder.AppendLine($"Name: {hotel.HotelName}");
            builder.AppendLine($"City: {hotel.City}");
            builder.AppendLine($"Address: {hotel.Address}");
            builder.AppendLine($"Admin: {hotel.AdminName}");
            builder.AppendLine($"Rooms: {hotel.TotalRooms}");
            builder.AppendLine($"Status: {hotel.Status}");
            builder.AppendLine($"Rating: {hotel.Rating}");

            MessageBox.Show(builder.ToString(), "Hotel details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task EditHotelAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            var editableHotel = new Hotel
            {
                HotelID = hotel.HotelID,
                HotelName = hotel.HotelName,
                Address = hotel.Address,
                City = hotel.City,
                Description = hotel.Description,
                Rating = hotel.Rating,
                MinPrice = hotel.MinPrice,
                MaxPrice = hotel.MaxPrice,
                IsVisible = hotel.IsVisible
            };

            var dialog = new EditHotelDialog
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                DataContext = editableHotel
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var storedHotel = await _hotelRepository.GetByIdAsync(hotel.HotelID) ?? hotel;
                storedHotel.HotelName = editableHotel.HotelName;
                storedHotel.Address = editableHotel.Address;
                storedHotel.City = editableHotel.City;
                storedHotel.Description = editableHotel.Description;
                storedHotel.Rating = editableHotel.Rating;
                storedHotel.MinPrice = editableHotel.MinPrice;
                storedHotel.MaxPrice = editableHotel.MaxPrice;
                storedHotel.IsVisible = editableHotel.IsVisible;

                await _hotelRepository.UpdateAsync(storedHotel);

                hotel.HotelName = storedHotel.HotelName;
                hotel.Address = storedHotel.Address;
                hotel.City = storedHotel.City;
                hotel.Description = storedHotel.Description;
                hotel.Rating = storedHotel.Rating;
                hotel.MinPrice = storedHotel.MinPrice;
                hotel.MaxPrice = storedHotel.MaxPrice;
                hotel.IsVisible = storedHotel.IsVisible;
                hotel.Status = GetHotelStatus(hotel);

                UpdateCityOptions();
                UpdateHotelStats();
                ApplyHotelFilters();

                ShowNotification("Hotel information updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update hotel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ManageHotelRoomsAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            if (_allRooms.Count == 0)
            {
                _allRooms = await _roomRepository.GetAllAsync();
            }

            var rooms = _allRooms.Where(r => r.HotelID == hotel.HotelID).ToList();

            if (rooms.Count == 0)
            {
                MessageBox.Show("No rooms are currently registered for this hotel.", "Room management", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Total rooms: {rooms.Count}");
            foreach (var group in rooms.GroupBy(r => r.RoomType).OrderBy(g => g.Key))
            {
                builder.AppendLine($" • {group.Key}: {group.Count()} room(s)");
            }

            MessageBox.Show(builder.ToString(), "Room overview", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ShowHotelAnalytics(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            var hotelBookings = _allBookings.Where(b => b.HotelID == hotel.HotelID).ToList();
            if (hotelBookings.Count == 0)
            {
                MessageBox.Show("There are no bookings for this hotel yet.", "Hotel analytics", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var bookingIds = new HashSet<string>(hotelBookings.Select(b => b.BookingID));
            var revenue = _allPayments.Where(p => bookingIds.Contains(p.BookingID)).Sum(p => p.TotalPayment);
            var confirmed = hotelBookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase));
            var completed = hotelBookings.Count(b => string.Equals(b.Status, "Completed", StringComparison.OrdinalIgnoreCase));
            var pending = hotelBookings.Count(b => string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            var builder = new StringBuilder();
            builder.AppendLine($"Total bookings: {hotelBookings.Count}");
            builder.AppendLine($"Confirmed: {confirmed}");
            builder.AppendLine($"Completed: {completed}");
            builder.AppendLine($"Pending: {pending}");
            builder.AppendLine($"Revenue: {revenue:C}");

            MessageBox.Show(builder.ToString(), "Hotel analytics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ToggleHotelVisibilityAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            if (!hotel.IsApproved)
            {
                MessageBox.Show("Only approved hotels can be suspended or reactivated.", "Hotel status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var storedHotel = await _hotelRepository.GetByIdAsync(hotel.HotelID) ?? hotel;
                storedHotel.IsVisible = !storedHotel.IsVisible;
                await _hotelRepository.UpdateAsync(storedHotel);

                hotel.IsVisible = storedHotel.IsVisible;
                hotel.Status = GetHotelStatus(hotel);

                UpdateHotelStats();
                ApplyHotelFilters();

                var statusText = hotel.IsVisible ? "Hotel has been reactivated." : "Hotel has been suspended.";
                ShowNotification(statusText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update hotel status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteHotelAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            var confirmation = MessageBox.Show(
                $"Are you sure you want to delete the hotel \"{hotel.HotelName}\"? This action cannot be undone.",
                "Confirm delete hotel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _hotelRepository.DeleteAsync(hotel.HotelID);
                await _hotelRepository.SaveAsync();

                var storedHotel = Hotels.FirstOrDefault(h => h.HotelID == hotel.HotelID);
                if (storedHotel != null)
                {
                    Hotels.Remove(storedHotel);
                }

                var pendingHotel = PendingHotels.FirstOrDefault(h => h.HotelID == hotel.HotelID);
                if (pendingHotel != null)
                {
                    PendingHotels.Remove(pendingHotel);
                }

                if (SelectedPendingHotel?.HotelID == hotel.HotelID)
                {
                    SelectedPendingHotel = PendingHotels.FirstOrDefault();
                }

                if (hotel.IsApproved && TotalHotels > 0)
                {
                    TotalHotels--;
                }

                UpdateHotelStats();
                UpdateCityOptions();
                ApplyHotelFilters();
                UpdatePendingCounts();

                ShowNotification("Hotel has been permanently deleted.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete hotel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ContactHotelAdminAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                ShowNotification("Unable to locate the selected hotel.");
                return;
            }

            var admin = Users.FirstOrDefault(u => u.UserID == hotel.UserID) ?? await _userRepository.GetByIdAsync(hotel.UserID);
            if (admin == null || string.IsNullOrWhiteSpace(admin.Email))
            {
                MessageBox.Show("The hotel administrator does not have a valid email address.", "Contact admin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var subject = Uri.EscapeDataString($"Regarding {hotel.HotelName}");
            var mailto = $"mailto:{admin.Email}?subject={subject}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = mailto,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                Clipboard.SetText(admin.Email);
                MessageBox.Show($"Unable to open the default mail client. The email address has been copied to the clipboard: {admin.Email}", "Contact admin", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ViewUserDetails(User? user)
        {
            if (user == null)
            {
                ShowNotification("Unable to locate the selected user.");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"User ID: {user.UserID}");
            builder.AppendLine($"Name: {user.FullName}");
            builder.AppendLine($"Email: {user.Email}");
            builder.AppendLine($"Phone: {user.Phone}");
            builder.AppendLine($"Gender: {user.Gender}");
            builder.AppendLine($"Role: {user.Role}");
            builder.AppendLine($"Date of Birth: {user.DateOfBirth:dd/MM/yyyy}");

            MessageBox.Show(builder.ToString(), "User details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task EditUserAsync(User? user)
        {
            if (user == null)
            {
                ShowNotification("Unable to locate the selected user.");
                return;
            }

            var editableUser = new User
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Phone = user.Phone,
                Gender = user.Gender,
                Email = user.Email,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth,
                AvatarUrl = user.AvatarUrl,
                Password = user.Password
            };

            var dialog = new EditUserDialog
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                DataContext = editableUser
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var storedUser = await _userRepository.GetByIdAsync(user.UserID) ?? user;
                storedUser.FullName = editableUser.FullName;
                storedUser.Phone = editableUser.Phone;
                storedUser.Gender = editableUser.Gender;
                storedUser.Role = editableUser.Role;
                storedUser.DateOfBirth = editableUser.DateOfBirth;

                await _userRepository.UpdateAsync(storedUser);

                user.FullName = storedUser.FullName;
                user.Phone = storedUser.Phone;
                user.Gender = storedUser.Gender;
                user.Role = storedUser.Role;
                user.DateOfBirth = storedUser.DateOfBirth;

                var index = Users.IndexOf(user);
                if (index >= 0)
                {
                    Users.RemoveAt(index);
                    Users.Insert(index, user);
                }

                UpdateUserRoleCounts();
                ApplyUserFilters();

                ShowNotification("User information updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeactivateUserAsync(User? user)
        {
            if (user == null)
            {
                ShowNotification("Unable to locate the selected user.");
                return;
            }

            if (CurrentUser != null && CurrentUser.UserID == user.UserID)
            {
                MessageBox.Show("You cannot deactivate your own account while logged in.", "User management", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Are you sure you want to deactivate {user.FullName}?", "Confirm deactivation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _userRepository.DeleteAsync(user.UserID);
                await _userRepository.SaveAsync();

                Users.Remove(user);
                TotalUsers = Math.Max(0, TotalUsers - 1);

                UpdateUserRoleCounts();
                ApplyUserFilters();

                ShowNotification("User has been deactivated.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to deactivate user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ContactUserAsync(User? user)
        {
            if (user == null)
            {
                ShowNotification("Unable to locate the selected user.");
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                MessageBox.Show("The selected user does not have a valid email address.", "Contact user", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var subject = Uri.EscapeDataString("Support from Super Admin");
            var mailto = $"mailto:{user.Email}?subject={subject}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = mailto,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                Clipboard.SetText(user.Email);
                MessageBox.Show($"Unable to open the default mail client. The email address has been copied to the clipboard: {user.Email}", "Contact user", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
