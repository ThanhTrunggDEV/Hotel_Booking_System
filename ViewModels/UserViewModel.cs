using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace Hotel_Booking_System.ViewModels
{
    public partial class UserViewModel : Bindable, IUserViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private readonly IAuthentication _authenticationSerivce;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly INavigationService _navigationService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IAIChatService _aiChatService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAIChatRepository _aiChatRepository;
        private readonly IReviewRepository _reviewRepository;

        private string userMail = string.Empty;
        private int _totalBookings;
        private double _totalSpent;
        private string _showSearchRoom = "Collapsed";
        private string _showSearchHotel = "Visible";
        private string _sortType = "Default";
        private string _roomSortType = "Default";
        private string _showAvailableHotels = "Visible";
        private string _showRooms = "Collapsed";
        private string _showRegisterForm = "Collapsed";
        private string _showChatBox = "Collapsed";
        private string _showChatButton = "Visible";
        private Hotel _currentHotel = new();
        private User _currentUser = new();
        private string _errorVisibility = "Collapsed";
        private string _errorMessage = string.Empty;
        private string _requestHotelName = "";
        private string _requestHotelAddress = "";
        private string _selectedModel = string.Empty;
        private string _chatInput = string.Empty;
        private string _membershipLevel = "Bronze";
        private string _hasBookings = "Collapsed";

        private DispatcherTimer? _typingTimer;

        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _notificationMessage = string.Empty;
        private DispatcherTimer? _notificationTimer;

        public string ShowSearchRoom { get => _showSearchRoom; set => Set(ref _showSearchRoom, value); }
        public string ShowSearchHotel { get => _showSearchHotel; set => Set(ref _showSearchHotel, value); }
        public double TotalSpent { get => _totalSpent; set => Set(ref _totalSpent, value); }

        public int TotalBookings { get => _totalBookings; set => Set(ref _totalBookings, value); }
        public string MembershipLevel { get => _membershipLevel; set => Set(ref _membershipLevel, value); }
        public string HasBookings { get => _hasBookings; set => Set(ref _hasBookings, value); }
        public string CurrentPassword { get => _currentPassword; set => Set(ref _currentPassword, value); }
        public string NewPassword { get => _newPassword; set => Set(ref _newPassword, value); }
        public string ConfirmPassword { get => _confirmPassword; set => Set(ref _confirmPassword, value); }
        public string NotificationMessage { get => _notificationMessage; set => Set(ref _notificationMessage, value); }

        public string SortType {
            get => _sortType;
            set
            {
                Set(ref _sortType, value);
                SortHotels();
            }
        }

        public string RoomSortType
        {
            get => _roomSortType;
            set
            {
                Set(ref _roomSortType, value);
                SortRooms();
            }
        }
        public User CurrentUser { get => _currentUser; set => Set(ref _currentUser, value); }
        public Hotel CurrentHotel { get => _currentHotel; set => Set(ref _currentHotel, value); }
        public string RequestHotelName { get => _requestHotelName; set => Set(ref _requestHotelName, value); }
        public string RequestHotelAddress { get => _requestHotelAddress; set => Set(ref _requestHotelAddress, value); }

        public string ShowChatBox
        {
            get => _showChatBox;
            set => Set(ref _showChatBox, value);
        }
        public string ShowChatButton
        {
            get => _showChatButton;
            set => Set(ref _showChatButton, value);
        }
        public string ShowRegisterForm
        {
            get => _showRegisterForm;
            set => Set(ref _showRegisterForm, value);
        }
        public string ShowAvailableHotels
        {
            get => _showAvailableHotels;
            set => Set(ref _showAvailableHotels, value);
        }

        public string ShowRoomList
        {
            get => _showRooms;
            set => Set(ref _showRooms, value);
        }

        public ObservableCollection<Booking> AllBookings { get; set; } = new();
        public ObservableCollection<Booking> Bookings { get; set; } = new();

        public ObservableCollection<string> BookingStatusOptions { get; } = new()
        {
            "All",
            "Pending",
            "Confirmed",
            "Cancelled",
            "CancelledRequested",
            "ModifyRequested",
            "Done"
        };

        private string _selectedBookingStatus = "All";
        public string SelectedBookingStatus
        {
            get => _selectedBookingStatus;
            set
            {
                Set(ref _selectedBookingStatus, value);
                ApplyBookingFilter();
            }
        }

        public ObservableCollection<Hotel> Hotels
        {
            get;
            set;
        } = new();

        public ObservableCollection<Room> Rooms
        {
            get;
            set;
        } = new();

        public ObservableCollection<Room> FilteredRooms
        {
            get;
            set;
        } = new();

        public string ErrorVisibility { get => _errorVisibility; set => Set(ref _errorVisibility, value); }
        public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }
        public string ChatInput { get => _chatInput; set => Set(ref _chatInput, value); }
        public ObservableCollection<AIChat> Chats { get; set; } = new();
        public ObservableCollection<Review> Reviews { get; set; } = new ObservableCollection<Review>();
         public string SelectedModel { get => _selectedModel; set => Set(ref _selectedModel, value); }

  



        public UserViewModel(IPaymentRepository paymentRepository,IAIChatRepository aIChatRepository ,IAIChatService aIChatService , IBookingRepository bookingRepository ,IUserRepository userRepository, IHotelRepository hotelRepository, INavigationService navigationService, IRoomRepository roomRepository, IAuthentication authentication, IHotelAdminRequestRepository hotelAdminRequestRepository, IReviewRepository reviewRepository)
        {
            _aiChatService = aIChatService;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _authenticationSerivce = authentication;
            _navigationService = navigationService;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _aiChatRepository = aIChatRepository;
            _reviewRepository = reviewRepository;

            WeakReferenceMessenger.Default.Register<UserViewModel, MessageService>(
     this,
     (recipient, message) =>
     {
         recipient.userMail = message.Value;
         GetCurrentUser();
         FilterChatsByUser(CurrentUser.UserID);
         FilterBookingsByUser(CurrentUser.UserID);
     });


          //  SeedData();

            var hotelList = _hotelRepository.GetAllAsync().Result
                .Where(h => h.IsApproved && h.IsVisible).ToList();
            CalculateAverageRatings(hotelList);
            Hotels = new ObservableCollection<Hotel>(hotelList);
            Rooms = new ObservableCollection<Room>(_roomRepository.GetAllAsync().Result);



            FilteredRooms = new ObservableCollection<Room>();

            //_userRepository.AddAsync(new User
            //{
            //    UserID = Guid.NewGuid().ToString(),
            //    FullName = "NTT",
            //    DateOfBirth = DateTime.Now,
            //    Email = "admin@gmail.com",
            //    Password = authentication.HashPassword("123"),
            //    Gender = "Male",
            //    Role = "SuperAdmin",
            //    Phone = "0971632201"
            //});
            //_userRepository.SaveAsync();
        }
        

        private void FilterChatsByUser(string userId)
        {
            var data = _aiChatRepository.GetByUserId(userId).Result;
            foreach(var chat in data)
            {
                Chats.Add(chat);
            }
        }

        private void SortHotels()
        {
            if (SortType == "User Rating: High to Low")
            {
                var sorted = Hotels.OrderByDescending(h => h.AverageRating).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "User Rating: Low to High")
            {
                var sorted = Hotels.OrderBy(h => h.AverageRating).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Hotel Rating: High to Low")
            {
                var sorted = Hotels.OrderByDescending(h => h.Rating).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Hotel Rating: Low to High")
            {
                var sorted = Hotels.OrderBy(h => h.Rating).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Name: A to Z")
            {
                var sorted = Hotels.OrderBy(h => h.HotelName).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if(SortType == "Price: Low to High")
            {
                var sorted = Hotels.OrderBy(h => h.MinPrice).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Price: High to Low")
            {
                var sorted = Hotels.OrderByDescending(h => h.MaxPrice).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Name: Z to A")
            {
                var sorted = Hotels.OrderByDescending(h => h.HotelName).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }

        }

        private void CalculateAverageRatings(IEnumerable<Hotel> hotels)
        {
            var reviewList = _reviewRepository.GetAllAsync().Result;
            var averages = reviewList
                .GroupBy(r => r.HotelID)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            foreach (var hotel in hotels)
            {
                hotel.AverageRating = averages.TryGetValue(hotel.HotelID, out var avg)
                    ? Math.Round(avg, 1)
                    : 0;
            }
        }

        private void SortRooms()
        {
            List<Room> sorted;
            if (RoomSortType == "Price: Low to High")
            {
                sorted = FilteredRooms.OrderBy(r => r.PricePerNight).ToList();
            }
            else if (RoomSortType == "Price: High to Low")
            {
                sorted = FilteredRooms.OrderByDescending(r => r.PricePerNight).ToList();
            }
            else if (RoomSortType == "Capacity: Low to High")
            {
                sorted = FilteredRooms.OrderBy(r => r.Capacity).ToList();
            }
            else if (RoomSortType == "Capacity: High to Low")
            {
                sorted = FilteredRooms.OrderByDescending(r => r.Capacity).ToList();
            }
            else
            {
                return;
            }

            FilteredRooms.Clear();
            foreach (var room in sorted)
            {
                FilteredRooms.Add(room);
            }
        }
        private void GetCurrentUser()
        {
            var user = _userRepository.GetByEmailAsync(userMail).Result;
            if (user != null)
            {
                CurrentUser = user;
            }
        }
        [RelayCommand]
        private void SearchHotels(object parameter)
        {
             
            if (parameter is Dictionary<string, object?> searchParams)
            {
                string location = searchParams.TryGetValue("Location", out var loc) ? loc?.ToString() ?? string.Empty : string.Empty;
                double? minPrice = searchParams.TryGetValue("MinPrice", out var min) && min is double dmin ? dmin : null;
                double? maxPrice = searchParams.TryGetValue("MaxPrice", out var max) && max is double dmax ? dmax : null;
                bool? fiveStar = searchParams.TryGetValue("FiveStar", out var five) ? five as bool? : null;
                bool? fourStar = searchParams.TryGetValue("FourStar", out var four) ? four as bool? : null;
                bool? threeStar = searchParams.TryGetValue("ThreeStar", out var three) ? three as bool? : null;
                bool? twoStar = searchParams.TryGetValue("TwoStar", out var two) ? two as bool? : null;
                bool? oneStar = searchParams.TryGetValue("OneStar", out var one) ? one as bool? : null;
                bool? freeWifi = searchParams.TryGetValue("FreeWifi", out var wifi) ? wifi as bool? : null;
                bool? swimmingPool = searchParams.TryGetValue("SwimmingPool", out var pool) ? pool as bool? : null;
                bool? freeParking = searchParams.TryGetValue("FreeParking", out var parking) ? parking as bool? : null;
                bool? restaurant = searchParams.TryGetValue("Restaurant", out var rest) ? rest as bool? : null;
                bool? gym = searchParams.TryGetValue("Gym", out var gymVal) ? gymVal as bool? : null;
                double? userRating = searchParams.TryGetValue("UserRating", out var ur) && ur is double dur ? dur : null;


                List<Hotel> hotels = _hotelRepository.GetAllAsync().Result
                    .Where(h => h.IsApproved && h.IsVisible).ToList();
                CalculateAverageRatings(hotels);



                if (!string.IsNullOrWhiteSpace(location))
                {
                    hotels = hotels.Where(h => h.City.Contains(location, StringComparison.OrdinalIgnoreCase) || h.Address.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                
                if (minPrice.HasValue)
                    hotels = hotels.Where(h => h.MinPrice >= minPrice.Value).ToList();
                if (maxPrice.HasValue)
                    hotels = hotels.Where(h => h.MaxPrice <= maxPrice.Value).ToList();

                var starFilters = new List<int>();
                if (fiveStar == true) starFilters.Add(5);
                if (fourStar == true) starFilters.Add(4);
                if (threeStar == true) starFilters.Add(3);
                if (twoStar == true) starFilters.Add(2);
                if (oneStar == true) starFilters.Add(1);
                if (starFilters.Count > 0)
                    hotels = hotels.Where(h => starFilters.Contains(h.Rating)).ToList();

                if (userRating.HasValue)
                    hotels = hotels.Where(h => h.AverageRating >= userRating.Value).ToList();

                if (freeWifi == true)
                    hotels = hotels.Where(h => h.Amenities.Any(a => a.AmenityName == "Free WiFi")).ToList();
                if (swimmingPool == true)
                    hotels = hotels.Where(h => h.Amenities.Any(a => a.AmenityName == "Swimming Pool")).ToList();
                if (freeParking == true)
                    hotels = hotels.Where(h => h.Amenities.Any(a => a.AmenityName == "Free Parking")).ToList();
                if (restaurant == true)
                    hotels = hotels.Where(h => h.Amenities.Any(a => a.AmenityName == "Restaurant")).ToList();
                if (gym == true)
                    hotels = hotels.Where(h => h.Amenities.Any(a => a.AmenityName == "Gym")).ToList();



                Hotels.Clear();
                for (int i = 0; i < hotels.Count; i++)
                {
                    Hotels.Add(hotels[i]);
                }

            }
            


        }

        [RelayCommand]
        private void FilterRooms(object parameter)
        {
            if (parameter is Dictionary<string, object?> filterParams && CurrentHotel != null)
            {
                double? price = filterParams.TryGetValue("Price", out var p) && p is double dp ? dp : null;
                int? capacity = filterParams.TryGetValue("Capacity", out var cap) && cap is int icap ? icap : null;
                DateTime? checkIn = filterParams.TryGetValue("CheckIn", out var ci) && ci is DateTime dci ? dci : null;
                DateTime? checkOut = filterParams.TryGetValue("CheckOut", out var co) && co is DateTime dco ? dco : null;

                var rooms = Rooms.Where(r => r.HotelID == CurrentHotel.HotelID).ToList();

                if (price.HasValue)
                    rooms = rooms.Where(r => r.PricePerNight <= price.Value).ToList();
                if (capacity.HasValue)
                    rooms = rooms.Where(r => r.Capacity >= capacity.Value).ToList();

                var bookings = _bookingRepository.GetAllAsync().Result;

                if (checkIn.HasValue && checkOut.HasValue)
                {
                    rooms = rooms
                        .Where(r => !bookings.Any(b => b.RoomID == r.RoomID && b.Status != "Cancelled" &&
                                                       checkIn < b.CheckOutDate && checkOut > b.CheckInDate))
                        .ToList();

                    foreach (var room in rooms)
                    {
                        EvaluateRoomAvailability(room, bookings, checkIn, checkOut);
                    }
                }
                else
                {
                    foreach (var room in rooms)
                    {
                        EvaluateRoomAvailability(room, bookings, null, null);
                    }
                }

                FilteredRooms.Clear();
                foreach (var room in rooms)
                    FilteredRooms.Add(room);
                SortRooms();
            }
        }

        [RelayCommand]
        private void ClearRoomFilters()
        {
            if (CurrentHotel != null)
                FilterRoomsByHotel(CurrentHotel.HotelID);
        }

        [RelayCommand]
        private void ClearSearch()
        {
            Hotels.Clear();
            var allHotels = _hotelRepository.GetAllAsync().Result
                .Where(h => h.IsApproved && h.IsVisible).ToList();
            CalculateAverageRatings(allHotels);
            foreach (var hotel in allHotels)
            {
                Hotels.Add(hotel);
            }
        }

        [RelayCommand]
        private void ShowChatButtonFunc()
        {
            ShowChatButton = "Visible";
            ShowChatBox = "Collapsed";
        }
        [RelayCommand]
        private void ShowChatBoxFunc()
        {
            ShowChatButton = "Collapsed";
            ShowChatBox = "Visible";
        }
        [RelayCommand]
        private async Task Send()
        {
            var message = ChatInput;
            if (string.IsNullOrWhiteSpace(message) || CurrentUser == null)
                return;

            ChatInput = string.Empty;
            ErrorVisibility = "Collapsed";
            ErrorMessage = string.Empty;

            var chat = new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = CurrentUser.UserID,
                Message = message,
                CreatedAt = DateTime.Now,
                IsTyping = true
            };
            Chats.Add(chat);
            StartTypingIndicator(chat);

            try
            {
                var responseChat = await _aiChatService.SendAsync(CurrentUser.UserID, message);
                StopTypingIndicator(chat);
                chat.ChatID = responseChat.ChatID;
                chat.CreatedAt = responseChat.CreatedAt;
                await TypeOutResponse(chat, responseChat.Response);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorVisibility = "Visible";
                Chats.Remove(chat);
            }
        }

        private void StartTypingIndicator(AIChat chat)
        {
            chat.TypingIndicator = string.Empty;
            int dotCount = 0;
            _typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            _typingTimer.Tick += (s, e) =>
            {
                dotCount = (dotCount + 1) % 4;
                chat.TypingIndicator = new string('.', dotCount);
            };
            _typingTimer.Start();
        }

        private void StopTypingIndicator(AIChat chat)
        {
            if (_typingTimer != null)
            {
                _typingTimer.Stop();
                _typingTimer = null;
            }
            chat.IsTyping = false;
            chat.TypingIndicator = string.Empty;
        }

        private async Task TypeOutResponse(AIChat chat, string response)
        {
            chat.IsTyping = false;
            chat.Response = string.Empty;
            foreach (var c in response)
            {
                chat.Response += c;
                await Task.Delay(30);
            }
        }


        [RelayCommand]
        private void ShowHotelDetails(string hotelID)
        {

            CurrentHotel = Hotels.FirstOrDefault(h => h.HotelID == hotelID);
            FilterRoomsByHotel(hotelID);
            LoadReviewsForHotel(hotelID);
            ShowAvailableHotels = "Collapsed";
            ShowSearchHotel = "Collapsed";

            ShowSearchRoom = "Visible";
            ShowRoomList = "Visible";
        }
        [RelayCommand]
        private void Register()
        {
            ShowRegisterForm = "Visible";
        }
        [RelayCommand]
        private void CloseForm()
        {
            ShowRegisterForm = "Collapsed";
        }
        [RelayCommand]
        private async Task SubmitRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestHotelName) || string.IsNullOrWhiteSpace(RequestHotelAddress)) return;
            var req = new HotelAdminRequest
            {
                RequestID = Guid.NewGuid().ToString(),
                UserID = CurrentUser.UserID,
                HotelName = RequestHotelName,
                HotelAddress = RequestHotelAddress,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            await _hotelAdminRequestRepository.AddAsync(req);
            await _hotelAdminRequestRepository.SaveAsync();
            RequestHotelName = RequestHotelAddress = string.Empty;
            ShowRegisterForm = "Collapsed";
        }

        [RelayCommand]
        private void HideRooms()
        {
            ShowRoomList = "Collapsed";
            ShowAvailableHotels = "Visible";

            ShowSearchHotel = "Visible";
            ShowSearchRoom = "Collapsed";
        }
        [RelayCommand]
        private async Task UploadImage()
        {
            FileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentUser.AvatarUrl = await UploadImageService.UploadAsync(openFileDialog.FileName);
                await _userRepository.UpdateAsync(CurrentUser);
                var refreshed = await _userRepository.GetByIdAsync(CurrentUser.UserID);
                if (refreshed != null)
                {
                    CurrentUser = refreshed;
                }
            }
        }

        [RelayCommand]
        private void UpdateInfo()
        {
            _userRepository.UpdateAsync(CurrentUser);

        }
        [RelayCommand]
        private async Task ChangePassword()
        {
            if (CurrentUser == null)
                return;

            if (!_authenticationSerivce.VerifyPassword(CurrentPassword, CurrentUser.Password))
            {
                ShowNotification("Current password is incorrect.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                ShowNotification("New password must be at least 6 characters.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ShowNotification("New password and confirmation do not match.");
                return;
            }

            CurrentUser.Password = _authenticationSerivce.HashPassword(NewPassword);
            await _userRepository.UpdateAsync(CurrentUser);

            ShowNotification("Password changed successfully.");
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
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
                _notificationTimer.Stop();
            };
            _notificationTimer.Start();
        }
        [RelayCommand]
        private void Logout()
        {

            _navigationService.NavigateToLogin();
        }
        [RelayCommand]
        private void BookRoom(Room room)
        {
            var hotel = Hotels.FirstOrDefault(h => h.HotelID == room.HotelID);
            if (hotel == null)
            {
                return;
            }

            bool res = _navigationService.OpenBookingDialog(room, CurrentUser, hotel);

            if (res)
            {
                FilterRoomsByHotel(room.HotelID);
                FilterBookingsByUser(CurrentUser.UserID);
            }
        }
        [RelayCommand]
        private void ShowBookedDates(Room room)
        {
            if (room == null)
                return;

            var bookings = _bookingRepository.GetAllAsync().Result;
            var roomBookings = bookings
                .Where(b => b.RoomID == room.RoomID && b.Status != "Cancelled")
                .OrderBy(b => b.CheckInDate)
                .ToList();

            if (roomBookings.Any())
            {
                var message = string.Join("\n", roomBookings.Select(b =>
                    $"{b.CheckInDate:dd/MM/yyyy} - {b.CheckOutDate:dd/MM/yyyy}"));
                MessageBox.Show(message, "Booked Dates", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No bookings for this room yet.", "Booked Dates", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        [RelayCommand]
        private void ReviewBooking(Booking booking)
        {
            if (booking == null)
                return;

            // Only allow review after checkout date
            if (DateTime.Now < booking.CheckOutDate)
            {
                MessageBox.Show("You can only review after your stay has completed.", "Review not available", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reviews = _reviewRepository.GetAllAsync().Result;
            if (reviews.Any(r => r.BookingID == booking.BookingID))
            {
                MessageBox.Show("You have already reviewed this booking.", "Review Exists", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool res = _navigationService.OpenReviewDialog(booking);
            if (res)
            {
                LoadReviewsForHotel(booking.HotelID);
            }
        }

        [RelayCommand]
        private async Task CancelBooking(Booking booking)
        {
            if (booking == null)
                return;

            if (booking.Status == "Confirmed")
            {
                // Send cancellation request to hotel admin
                booking.Status = "CancelledRequested";
                MessageBox.Show("Cancellation request sent to hotel admin.", "Request sent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (booking.Status == "Pending")
            {
                // Cancel immediately if booking has not been confirmed yet
                booking.Status = "Cancelled";
                MessageBox.Show("Booking cancelled successfully.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await _bookingRepository.UpdateAsync(booking);
            FilterBookingsByUser(CurrentUser.UserID);
        }

        [RelayCommand]
        private async Task EditBooking(Booking booking)
        {
            if (booking == null)
                return;

            if (booking.Status == "Confirmed")
            {
                bool res = _navigationService.OpenModifyDialog(booking);
                if (res)
                {
                    // After modification a confirmed booking becomes pending again for admin approval
                    booking.Status = "ModifyRequested";
                    MessageBox.Show("Modification request sent to hotel admin.", "Request sent", MessageBoxButton.OK, MessageBoxImage.Information);
                    await _bookingRepository.UpdateAsync(booking);
                }

                
            }
            else if (booking.Status == "Pending")
            {
                bool res = _navigationService.OpenModifyDialog(booking);
                if (res)
                {
                    // Pending bookings can be modified directly
                    await _bookingRepository.UpdateAsync(booking);
                }
            }

            FilterBookingsByUser(CurrentUser.UserID);
        }
        
        private void FilterBookingsByUser(string userId)
        {
            var bookingList = _bookingRepository.GetBookingByUserId(userId).Result;
            UpdateCompletedBookings(bookingList);
            var payments = _paymentRepository.GetAllAsync().Result;
            AllBookings.Clear();
            double totalSpent = 0;
            var userBookings = bookingList.Where(b => b.UserID == userId).ToList();
            foreach (var booking in userBookings)
            {
                AllBookings.Add(booking);
                var payment = payments.FirstOrDefault(p => p.BookingID == booking.BookingID);
                if (payment != null)
                {
                    totalSpent += payment.TotalPayment;
                }
            }

            TotalSpent = totalSpent;
            TotalBookings = AllBookings.Count;
            MembershipLevel = GetMembershipLevel(totalSpent);
            ApplyBookingFilter();
        }

        private void ApplyBookingFilter()
        {
            Bookings.Clear();
            IEnumerable<Booking> filtered = AllBookings;
            if (SelectedBookingStatus != "All")
            {
                filtered = filtered.Where(b => b.Status == SelectedBookingStatus);
            }

            foreach (var booking in filtered)
            {
                Bookings.Add(booking);
            }

            HasBookings = Bookings.Count == 0 ? "Visible" : "Collapsed";
        }

        private void UpdateCompletedBookings(IEnumerable<Booking> bookings)
        {
            foreach (var booking in bookings)
            {
                if (booking.CheckOutDate.Date < DateTime.Today &&
                    booking.Status != "Cancelled" &&
                    booking.Status != "CancelledRequested" &&
                    booking.Status != "Done")
                {
                    booking.Status = "Done";
                    _bookingRepository.UpdateAsync(booking).Wait();
                }
            }
        }

        private string GetMembershipLevel(double totalSpent)
        {
            if (totalSpent >= 10000) return "Platinum";
            if (totalSpent >= 5000) return "Gold";
            if (totalSpent >= 1000) return "Silver";
            return "Bronze";
        }
        private void FilterRoomsByHotel(string hotelId)
        {
            FilteredRooms.Clear();
            var hotelRooms = Rooms.Where(r => r.HotelID == hotelId).ToList();
            var bookings = _bookingRepository.GetAllAsync().Result;
            foreach (var room in hotelRooms)
            {
                EvaluateRoomAvailability(room, bookings, null, null);
                FilteredRooms.Add(room);
            }
            SortRooms();
        }

        private void EvaluateRoomAvailability(Room room, List<Booking> bookings, DateTime? checkIn, DateTime? checkOut)
        {
            var roomBookings = bookings.Where(b => b.RoomID == room.RoomID && b.Status != "Cancelled");
            bool available = true;

            if (checkIn.HasValue && checkOut.HasValue)
            {
                available = !roomBookings.Any(b => checkIn < b.CheckOutDate && checkOut > b.CheckInDate);
            }
            else
            {
                available = !roomBookings.Any(b => b.CheckOutDate > DateTime.Now);
            }

            room.Status = available ? "Available" : "Booked";
        }
        private void LoadReviewsForHotel(string hotelId)
        {
            Reviews.Clear();
            var reviewList = _reviewRepository.GetAllAsync().Result;
            var hotelReviews = reviewList
                .Where(r => r.HotelID == hotelId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            foreach (var review in hotelReviews)
            {
                var user = _userRepository.GetByIdAsync(review.UserID).Result;
                if (user != null)
                {
                    review.ReviewerName = user.FullName;
                    review.ReviewerAvatarUrl = user.AvatarUrl;
                }
                review.AdminReplyDraft = string.Empty;
                Reviews.Add(review);
            }
        }

        
        

    }
}
