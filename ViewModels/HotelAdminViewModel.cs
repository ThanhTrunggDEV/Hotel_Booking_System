using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.Views;
using Hotel_Manager.FrameWorks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Hotel_Booking_System.ViewModels
{
    public partial class HotelAdminViewModel : Bindable, IHotelAdminViewModel
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly INavigationService _navigationService;
        private readonly IAuthentication _authenticationService;

        private string _userEmail = string.Empty;
        private User _currentUser = new();
        private Hotel? _currentHotel;
        private bool _isSyncingAmenities;

        private bool _hasFreeWifi;
        private bool _hasSwimmingPool;
        private bool _hasFreeParking;
        private bool _hasRestaurant;
        private bool _hasGym;

        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _notificationMessage = string.Empty;
        private double _totalSpent;
        private int _totalBookings;
        private int _pendingBookingsCount;
        private int _confirmedBookingsCount;
        private int _cancellationRequestsCount;
        private int _todayCheckInsCount;
        private int _hotelBookingsCount;
        private string _bookingSearchQuery = string.Empty;
        private string _membershipLevel = "Bronze";
        private DispatcherTimer? _notificationTimer;

        public ObservableCollection<Hotel> Hotels { get; } = new();
        public ObservableCollection<string> CityOptions { get; } = new();
        public Hotel? CurrentHotel
        {
            get => _currentHotel;
            set
            {
                Set(ref _currentHotel, value);

                EnsureCityInOptions(_currentHotel?.City);
                SyncAmenitiesFromHotel();
                _ = LoadRoomsAsync();
                _ = LoadBookingsAsync();
                _ = LoadReviewsAsync();
            }
        }

        private readonly List<Booking> _allBookings = new();
        private List<Booking> _cachedAllBookings = new();
        private List<Payment> _cachedPayments = new();

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Booking> Bookings { get; } = new();
        public ObservableCollection<string> BookingStatusFilters { get; } = new();
        public ObservableCollection<RevenueDataPoint> WeeklyRevenue { get; } = new();
        public ObservableCollection<RevenueDataPoint> MonthlyRevenue { get; } = new();
        public ObservableCollection<RevenueDataPoint> YearlyRevenue { get; } = new();

        private double _maxWeeklyRevenue;
        public double MaxWeeklyRevenue
        {
            get => _maxWeeklyRevenue;
            private set => Set(ref _maxWeeklyRevenue, value);
        }

        private double _maxMonthlyRevenue;
        public double MaxMonthlyRevenue
        {
            get => _maxMonthlyRevenue;
            private set => Set(ref _maxMonthlyRevenue, value);
        }

        private double _maxYearlyRevenue;
        public double MaxYearlyRevenue
        {
            get => _maxYearlyRevenue;
            private set => Set(ref _maxYearlyRevenue, value);
        }

        private double _weeklyRevenueTotal;
        public double WeeklyRevenueTotal
        {
            get => _weeklyRevenueTotal;
            private set => Set(ref _weeklyRevenueTotal, value);
        }

        private double _monthlyRevenueTotal;
        public double MonthlyRevenueTotal
        {
            get => _monthlyRevenueTotal;
            private set => Set(ref _monthlyRevenueTotal, value);
        }

        private double _yearlyRevenueTotal;
        public double YearlyRevenueTotal
        {
            get => _yearlyRevenueTotal;
            private set => Set(ref _yearlyRevenueTotal, value);
        }

        private readonly Dictionary<RevenueRange, List<RevenueDataPoint>> _revenueData = new();
        private List<Payment> _currentHotelPayments = new();

        public ObservableCollection<RevenueFilterOption> RevenueFilterOptions { get; } = new();

        private RevenueFilterOption? _selectedRevenueFilter;
        public RevenueFilterOption? SelectedRevenueFilter
        {
            get => _selectedRevenueFilter;
            set
            {
                Set(ref _selectedRevenueFilter, value);
                UpdateRevenueChart();
            }
        }

        private PlotModel _revenuePlotModel = CreateEmptyRevenuePlotModel();
        public PlotModel RevenuePlotModel
        {
            get => _revenuePlotModel;
            private set => Set(ref _revenuePlotModel, value);
        }

        private string _revenueSummary = "Chưa có dữ liệu doanh thu.";
        public string RevenueSummary
        {
            get => _revenueSummary;
            private set => Set(ref _revenueSummary, value);
        }

        private string _selectedBookingStatusFilter = "All";
        public string SelectedBookingStatusFilter
        {
            get => _selectedBookingStatusFilter;
            set
            {
                if (_selectedBookingStatusFilter == value)
                {
                    return;
                }

                Set(ref _selectedBookingStatusFilter, value);
                ApplyBookingFilter();
            }
        }
        public ObservableCollection<Review> Reviews { get; } = new();

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
            private set => Set(ref _notificationMessage, value);
        }

        public double TotalSpent
        {
            get => _totalSpent;
            private set => Set(ref _totalSpent, value);
        }

        public int TotalBookings
        {
            get => _totalBookings;
            private set => Set(ref _totalBookings, value);
        }

        public int PendingBookingsCount
        {
            get => _pendingBookingsCount;
            private set => Set(ref _pendingBookingsCount, value);
        }

        public int ConfirmedBookingsCount
        {
            get => _confirmedBookingsCount;
            private set => Set(ref _confirmedBookingsCount, value);
        }

        public int CancellationRequestsCount
        {
            get => _cancellationRequestsCount;
            private set => Set(ref _cancellationRequestsCount, value);
        }

        public int TodayCheckInsCount
        {
            get => _todayCheckInsCount;
            private set => Set(ref _todayCheckInsCount, value);
        }

        public int HotelBookingsCount
        {
            get => _hotelBookingsCount;
            private set => Set(ref _hotelBookingsCount, value);
        }

        public string BookingSearchQuery
        {
            get => _bookingSearchQuery;
            set
            {
                Set(ref _bookingSearchQuery, value);
                ApplyBookingFilter();
            }
        }

        public string MembershipLevel
        {
            get => _membershipLevel;
            private set => Set(ref _membershipLevel, value);
        }

        public bool HasFreeWifi
        {
            get => _hasFreeWifi;
            set
            {
                if (_hasFreeWifi == value)
                    return;

                Set(ref _hasFreeWifi, value);
                if (!_isSyncingAmenities)
                {
                    UpdateHotelAmenity("Free WiFi", value);
                }
            }
        }

        public bool HasSwimmingPool
        {
            get => _hasSwimmingPool;
            set
            {
                if (_hasSwimmingPool == value)
                    return;

                Set(ref _hasSwimmingPool, value);
                if (!_isSyncingAmenities)
                {
                    UpdateHotelAmenity("Swimming Pool", value, "Pool");
                }
            }
        }

        public bool HasFreeParking
        {
            get => _hasFreeParking;
            set
            {
                if (_hasFreeParking == value)
                    return;

                Set(ref _hasFreeParking, value);
                if (!_isSyncingAmenities)
                {
                    UpdateHotelAmenity("Free Parking", value);
                }
            }
        }

        public bool HasRestaurant
        {
            get => _hasRestaurant;
            set
            {
                if (_hasRestaurant == value)
                    return;

                Set(ref _hasRestaurant, value);
                if (!_isSyncingAmenities)
                {
                    UpdateHotelAmenity("Restaurant", value);
                }
            }
        }

        public bool HasGym
        {
            get => _hasGym;
            set
            {
                if (_hasGym == value)
                    return;

                Set(ref _hasGym, value);
                if (!_isSyncingAmenities)
                {
                    UpdateHotelAmenity("Gym", value, "Fitness Center");
                }
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => Set(ref _currentUser, value);
        }

        private int _totalReviews;
        public int TotalReviews
        {
            get => _totalReviews;
            private set
            {
                if (_totalReviews == value)
                    return;

                Set(ref _totalReviews, value);
                OnPropertyChanged(nameof(FiveStarRatio));
                OnPropertyChanged(nameof(FourStarRatio));
                OnPropertyChanged(nameof(ThreeStarRatio));
                OnPropertyChanged(nameof(TwoStarRatio));
                OnPropertyChanged(nameof(OneStarRatio));
            }
        }

        private double _averageRating;
        public double AverageRating
        {
            get => _averageRating;
            private set => Set(ref _averageRating, value);
        }

        private int _fiveStarCount;
        public int FiveStarCount
        {
            get => _fiveStarCount;
            private set
            {
                if (_fiveStarCount == value)
                    return;

                Set(ref _fiveStarCount, value);
                OnPropertyChanged(nameof(FiveStarRatio));
            }
        }

        private int _fourStarCount;
        public int FourStarCount
        {
            get => _fourStarCount;
            private set
            {
                if (_fourStarCount == value)
                    return;

                Set(ref _fourStarCount, value);
                OnPropertyChanged(nameof(FourStarRatio));
            }
        }

        private int _threeStarCount;
        public int ThreeStarCount
        {
            get => _threeStarCount;
            private set
            {
                if (_threeStarCount == value)
                    return;

                Set(ref _threeStarCount, value);
                OnPropertyChanged(nameof(ThreeStarRatio));
            }
        }

        private int _twoStarCount;
        public int TwoStarCount
        {
            get => _twoStarCount;
            private set
            {
                if (_twoStarCount == value)
                    return;

                Set(ref _twoStarCount, value);
                OnPropertyChanged(nameof(TwoStarRatio));
            }
        }

        private int _oneStarCount;
        public int OneStarCount
        {
            get => _oneStarCount;
            private set
            {
                if (_oneStarCount == value)
                    return;

                Set(ref _oneStarCount, value);
                OnPropertyChanged(nameof(OneStarRatio));
            }
        }

        public double FiveStarRatio => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
        public double FourStarRatio => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
        public double ThreeStarRatio => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
        public double TwoStarRatio => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
        public double OneStarRatio => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;

        public HotelAdminViewModel(
            IReviewRepository reviewRepository,
            IUserRepository userRepository,
            IRoomRepository roomRepository,
            IHotelRepository hotelRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            INavigationService navigationService,
            IAuthentication authenticationService)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _roomRepository = roomRepository;
            _hotelRepository = hotelRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            InitializeCityOptions();

            RevenueFilterOptions.Add(new RevenueFilterOption { DisplayName = "Theo tuần", Range = RevenueRange.Weekly });
            RevenueFilterOptions.Add(new RevenueFilterOption { DisplayName = "Theo tháng", Range = RevenueRange.Monthly });
            RevenueFilterOptions.Add(new RevenueFilterOption { DisplayName = "Theo năm", Range = RevenueRange.Yearly });
            RevenueFilterOptions.Add(new RevenueFilterOption { DisplayName = "Tổng", Range = RevenueRange.Cumulative });

            WeakReferenceMessenger.Default.Register<HotelAdminViewModel, MessageService>(this, (recipient, message) =>
            {
                recipient._userEmail = message.Value;
                recipient.LoadCurrentUser();
            });
        }

        private static readonly string[] DefaultCityOptions = new[]
        {
            "Hà Nội",
            "Tuyên Quang",
            "Lào Cai",
            "Thái Nguyên",
            "Phú Thọ",
            "Bắc Ninh",
            "Hưng Yên",
            "Hải Phòng",
            "Ninh Bình",
            "Quảng Trị",
            "Đà Nẵng",
            "Quảng Ngãi",
            "Gia Lai",
            "Khánh Hòa",
            "Lâm Đồng",
            "Đăk Lăk",
            "TP. Hồ Chí Minh",
            "TP. Cần Thơ",
            "Vĩnh Long",
            "Đồng Tháp",
            "Cà Mau",
            "An Giang"
        };

        private void InitializeCityOptions()
        {
            CityOptions.Clear();
            foreach (var city in DefaultCityOptions)
            {
                CityOptions.Add(city);
            }
        }

        private async void LoadCurrentUser()
        {
            var user = await _userRepository.GetByEmailAsync(_userEmail);
            if (user != null)
            {
                CurrentUser = user;
                LoadHotels();
            }
        }

        private async void LoadHotels()
        {
            if (string.IsNullOrEmpty(CurrentUser.UserID))
                return;

            var hotels = await _hotelRepository.GetAllAsync();
            Hotels.Clear();
            CityOptions.Clear();
            foreach (var hotel in hotels.Where(h => h.UserID == CurrentUser.UserID && h.IsApproved))
            {
                Hotels.Add(hotel);
                EnsureCityInOptions(hotel.City);
            }

            CurrentHotel = Hotels.FirstOrDefault();
        }

        private async Task LoadRoomsAsync()
        {
            if (CurrentHotel == null)
            {
                Rooms.Clear();
                return;
            }

            var rooms = await _roomRepository.GetAllAsync();
            var relevantRooms = rooms
                .Where(r => r.HotelID == CurrentHotel.HotelID)
                .OrderBy(r => r.RoomNumber)
                .ToList();

            Rooms.Clear();
            foreach (var room in relevantRooms)
            {
                Rooms.Add(room);
            }
        }

        private async Task LoadBookingsAsync()
        {
            if (CurrentHotel == null)
            {
                _allBookings.Clear();
                Bookings.Clear();
                _cachedAllBookings = new List<Booking>();
                _cachedPayments = new List<Payment>();
                UpdateBookingStatusFilters();
                UpdateBookingInsights();
                UpdateProfileStatistics();
                UpdateRevenueAnalytics(new List<Booking>(), new List<Payment>());
                return;
            }

            var rooms = await _roomRepository.GetAllAsync();
            var roomNumbers = rooms
                .Where(r => r.HotelID == CurrentHotel.HotelID)
                .ToDictionary(r => r.RoomID, r => r.RoomNumber);

            var allBookings = await _bookingRepository.GetAllAsync();
            var currentHotelBookings = allBookings
                .Where(b => b.HotelID == CurrentHotel.HotelID)
                .OrderByDescending(b => b.CheckInDate)
                .ToList();

            _allBookings.Clear();
            foreach (var booking in currentHotelBookings)
            {
                booking.RoomNumber = roomNumbers.TryGetValue(booking.RoomID, out var number)
                    ? number
                    : booking.RoomID;
                _allBookings.Add(booking);
            }

            _cachedAllBookings = allBookings;

            UpdateBookingStatusFilters();
            ApplyBookingFilter();

            _cachedPayments = await _paymentRepository.GetAllAsync();
            UpdateRevenueAnalytics(currentHotelBookings, _cachedPayments);
            UpdateProfileStatistics();
        }

        private async void LoadReviews()
        {
            await LoadReviewsAsync();
        }

        public async Task LoadReviewsAsync()
        {
            if (CurrentHotel == null)
                return;

            var reviews = await _reviewRepository.GetAllAsync();
            var relevantReviews = reviews
                .Where(r => r.HotelID == CurrentHotel.HotelID)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var userIds = relevantReviews
                .Select(r => r.UserID)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            var users = await _userRepository.GetAllAsync();
            var userLookup = users
                .Where(u => userIds.Contains(u.UserID))
                .ToDictionary(u => u.UserID, u => u);

            Reviews.Clear();
            foreach (var review in relevantReviews)
            {
                if (userLookup.TryGetValue(review.UserID, out var user))
                {
                    review.ReviewerName = user.FullName;
                    review.ReviewerAvatarUrl = user.AvatarUrl;
                }

                review.AdminReplyDraft = string.Empty;
                Reviews.Add(review);
            }

            CalculateReviewStatistics();
        }

        private void CalculateReviewStatistics()
        {
            TotalReviews = Reviews.Count;
            AverageRating = TotalReviews > 0 ? Reviews.Average(r => r.Rating) : 0;

            FiveStarCount = Reviews.Count(r => r.Rating == 5);
            FourStarCount = Reviews.Count(r => r.Rating == 4);
            ThreeStarCount = Reviews.Count(r => r.Rating == 3);
            TwoStarCount = Reviews.Count(r => r.Rating == 2);
            OneStarCount = Reviews.Count(r => r.Rating == 1);


        }

        [RelayCommand]
        private async Task ReplyToReview(Review? review)
        {
            if (review == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(review.AdminReply))
            {
                return;
            }

            var reply = review.AdminReplyDraft?.Trim();
            if (string.IsNullOrWhiteSpace(reply))
            {
                return;
            }

            review.AdminReply = reply;
            review.AdminReplyDraft = string.Empty;

            await _reviewRepository.UpdateAsync(review);

            var index = Reviews.IndexOf(review);
            if (index >= 0)
            {
                Reviews[index] = review;
            }
        }

        private void UpdateBookingStatusFilters()
        {
            var statuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var booking in _allBookings)
            {
                if (!string.IsNullOrWhiteSpace(booking.Status))
                {
                    statuses.Add(booking.Status);
                }
            }

            var orderedStatuses = new List<string> { "All" };
            orderedStatuses.AddRange(statuses.OrderBy(status => status));

            BookingStatusFilters.Clear();
            foreach (var status in orderedStatuses)
            {
                BookingStatusFilters.Add(status);
            }

            if (!BookingStatusFilters.Contains(SelectedBookingStatusFilter))
            {
                SelectedBookingStatusFilter = "All";
            }
        }

        private void ApplyBookingFilter()
        {
            if (Bookings == null)
            {
                return;
            }

            Bookings.Clear();

            IEnumerable<Booking> filtered = _allBookings;
            if (!string.IsNullOrWhiteSpace(SelectedBookingStatusFilter) && SelectedBookingStatusFilter != "All")
            {
                filtered = filtered.Where(b => string.Equals(b.Status, SelectedBookingStatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(BookingSearchQuery))
            {
                filtered = filtered.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.BookingID) && b.BookingID.Contains(BookingSearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(b.GuestName) && b.GuestName.Contains(BookingSearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(b.RoomNumber) && b.RoomNumber.Contains(BookingSearchQuery, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (var booking in filtered)
            {
                Bookings.Add(booking);
            }

            UpdateBookingInsights();
        }

        private void UpdateBookingInsights()
        {
            var today = DateTime.Today;

            HotelBookingsCount = _allBookings.Count;
            PendingBookingsCount = _allBookings.Count(b => string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));
            ConfirmedBookingsCount = _allBookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase));
            CancellationRequestsCount = _allBookings.Count(b => string.Equals(b.Status, "CancelledRequested", StringComparison.OrdinalIgnoreCase));
            TodayCheckInsCount = _allBookings.Count(b => b.CheckInDate.Date == today);
        }

        private void UpdateProfileStatistics()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                TotalBookings = 0;
                TotalSpent = 0;
                MembershipLevel = "Bronze";
                return;
            }

            _cachedAllBookings ??= new List<Booking>();
            _cachedPayments ??= new List<Payment>();

            var userBookings = _cachedAllBookings
                .Where(b => string.Equals(b.UserID, CurrentUser.UserID, StringComparison.OrdinalIgnoreCase))
                .ToList();

            TotalBookings = userBookings.Count;

            if (userBookings.Count == 0)
            {
                TotalSpent = 0;
                MembershipLevel = "Bronze";
                return;
            }

            var bookingIds = new HashSet<string>(userBookings
                .Where(b => !string.IsNullOrEmpty(b.BookingID))
                .Select(b => b.BookingID));

            var totalSpent = _cachedPayments
                .Where(p => !string.IsNullOrEmpty(p.BookingID) && bookingIds.Contains(p.BookingID))
                .Sum(p => p.TotalPayment);

            TotalSpent = totalSpent;
            MembershipLevel = CalculateMembershipLevel(totalSpent);
        }

        private static string CalculateMembershipLevel(double totalRevenue)
        {
            if (totalRevenue >= 20000)
            {
                return "Platinum";
            }

            if (totalRevenue >= 10000)
            {
                return "Gold";
            }

            if (totalRevenue >= 5000)
            {
                return "Silver";
            }

            return "Bronze";
        }

        private void UpdateRevenueAnalytics(List<Booking> bookings, List<Payment> payments)
        {
            _currentHotelPayments = new List<Payment>();
            _revenueData.Clear();

            payments ??= new List<Payment>();

            if (bookings != null && bookings.Count > 0)
            {
                var bookingIds = bookings
                    .Select(b => b.BookingID)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToHashSet();

                if (bookingIds.Count > 0)
                {
                    _currentHotelPayments = payments
                        .Where(p => !string.IsNullOrWhiteSpace(p.BookingID) && bookingIds.Contains(p.BookingID))
                        .ToList();
                }
            }

            var today = DateTime.Today;

            _revenueData[RevenueRange.Weekly] = BuildWeeklyRevenue(_currentHotelPayments, today);
            _revenueData[RevenueRange.Monthly] = BuildMonthlyRevenue(_currentHotelPayments, today);
            _revenueData[RevenueRange.Yearly] = BuildYearlyRevenue(_currentHotelPayments, today);
            _revenueData[RevenueRange.Cumulative] = BuildCumulativeRevenue(_currentHotelPayments, today);

            UpdateYearlyRevenueOverview();

            if (SelectedRevenueFilter == null)
            {
                SelectedRevenueFilter = RevenueFilterOptions.FirstOrDefault();
            }
            else
            {
                UpdateRevenueChart();
            }
        }

        private List<RevenueDataPoint> BuildWeeklyRevenue(List<Payment> payments, DateTime referenceDate)
        {
            payments ??= new List<Payment>();
            var result = new List<RevenueDataPoint>();
            const int numberOfWeeks = 12;

            var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var currentWeekStart = referenceDate.Date;
            while (currentWeekStart.DayOfWeek != firstDayOfWeek)
            {
                currentWeekStart = currentWeekStart.AddDays(-1);
            }

            var startWeek = currentWeekStart.AddDays(-7 * (numberOfWeeks - 1));

            for (int i = 0; i < numberOfWeeks; i++)
            {
                var weekStart = startWeek.AddDays(i * 7);
                var weekEndExclusive = weekStart.AddDays(7);

                var amount = payments
                    .Where(p => p.PaymentDate >= weekStart && p.PaymentDate < weekEndExclusive)
                    .Sum(p => p.TotalPayment);

                var weekEndInclusive = weekEndExclusive.AddDays(-1);
                if (weekEndInclusive > referenceDate.Date)
                {
                    weekEndInclusive = referenceDate.Date;
                }

                result.Add(new RevenueDataPoint
                {
                    Label = $"{weekStart:dd/MM} - {weekEndInclusive:dd/MM}",
                    Amount = amount,
                    Timestamp = weekStart
                });
            }

            return result;
        }

        private List<RevenueDataPoint> BuildMonthlyRevenue(List<Payment> payments, DateTime referenceDate)
        {
            payments ??= new List<Payment>();
            var result = new List<RevenueDataPoint>();
            var startMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-11);

            for (int i = 0; i < 12; i++)
            {
                var currentMonthStart = startMonth.AddMonths(i);
                var nextMonthStart = currentMonthStart.AddMonths(1);
                var amount = payments
                    .Where(p => p.PaymentDate >= currentMonthStart && p.PaymentDate < nextMonthStart)
                    .Sum(p => p.TotalPayment);

                result.Add(new RevenueDataPoint
                {
                    Label = currentMonthStart.ToString("MM/yyyy"),
                    Amount = amount,
                    Timestamp = currentMonthStart
                });
            }

            return result;
        }

        private List<RevenueDataPoint> BuildYearlyRevenue(List<Payment> payments, DateTime referenceDate)
        {
            payments ??= new List<Payment>();
            var result = new List<RevenueDataPoint>();
            var startYear = new DateTime(referenceDate.Year - 4, 1, 1);

            for (int i = 0; i < 5; i++)
            {
                var currentYearStart = startYear.AddYears(i);
                var nextYearStart = currentYearStart.AddYears(1);
                var amount = payments
                    .Where(p => p.PaymentDate >= currentYearStart && p.PaymentDate < nextYearStart)
                    .Sum(p => p.TotalPayment);

                result.Add(new RevenueDataPoint
                {
                    Label = currentYearStart.Year.ToString(),
                    Amount = amount,
                    Timestamp = currentYearStart
                });
            }

            return result;
        }

        private List<RevenueDataPoint> BuildCumulativeRevenue(List<Payment> payments, DateTime referenceDate)
        {
            payments ??= new List<Payment>();
            var result = new List<RevenueDataPoint>();

            var grouped = payments
                .GroupBy(p => p.PaymentDate.Date)
                .OrderBy(g => g.Key);

            double runningTotal = 0;
            foreach (var group in grouped)
            {
                runningTotal += group.Sum(p => p.TotalPayment);
                result.Add(new RevenueDataPoint
                {
                    Label = group.Key.ToString("dd/MM/yyyy"),
                    Amount = runningTotal,
                    Timestamp = group.Key
                });
            }

            if (result.Count == 0)
            {
                result.Add(new RevenueDataPoint
                {
                    Label = referenceDate.ToString("dd/MM/yyyy"),
                    Amount = 0,
                    Timestamp = referenceDate
                });
            }

            return result;
        }

        private void UpdateRevenueChart()
        {
            if (SelectedRevenueFilter == null)
            {
                var emptyModel = CreateEmptyRevenuePlotModel();
                emptyModel.InvalidatePlot(true);
                RevenuePlotModel = emptyModel;
                RevenueSummary = "Chưa có dữ liệu doanh thu.";
                return;
            }

            if (!_revenueData.TryGetValue(SelectedRevenueFilter.Range, out var data) || data.Count == 0)
            {
                var emptyModel = CreateEmptyRevenuePlotModel();
                emptyModel.InvalidatePlot(true);
                RevenuePlotModel = emptyModel;
                RevenueSummary = "Chưa có dữ liệu doanh thu.";
                return;
            }

            var model = CreateRevenuePlotModel(data);
            model.InvalidatePlot(true);
            RevenuePlotModel = model;

            var firstDate = data.First().Timestamp;
            var lastDate = data.Last().Timestamp;
            var weeklyRangeEndDate = lastDate.AddDays(6);
            if (weeklyRangeEndDate > DateTime.Today)
            {
                weeklyRangeEndDate = DateTime.Today;
            }
            double total = SelectedRevenueFilter.Range == RevenueRange.Cumulative
                ? data.Last().Amount
                : data.Sum(d => d.Amount);

            string modeDescription = SelectedRevenueFilter.Range switch
            {
                RevenueRange.Weekly => "theo tuần",
                RevenueRange.Monthly => "theo tháng",
                RevenueRange.Yearly => "theo năm",
                RevenueRange.Cumulative => "tích lũy",
                _ => string.Empty
            };

            string rangeDescription = SelectedRevenueFilter.Range switch
            {
                RevenueRange.Weekly => $"({firstDate:dd/MM} - {weeklyRangeEndDate:dd/MM})",
                RevenueRange.Monthly => $"({firstDate:MM/yyyy} - {lastDate:MM/yyyy})",
                RevenueRange.Yearly => $"({firstDate:yyyy} - {lastDate:yyyy})",
                RevenueRange.Cumulative => $"(đến {lastDate:dd/MM/yyyy})",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(rangeDescription))
            {
                rangeDescription = " " + rangeDescription;
            }

            RevenueSummary = $"Tổng doanh thu {modeDescription}{rangeDescription}: ${total:N0}";
        }

        private void UpdateYearlyRevenueOverview()
        {
            YearlyRevenue.Clear();

            if (!_revenueData.TryGetValue(RevenueRange.Yearly, out var yearlyData) || yearlyData.Count == 0)
            {
                YearlyRevenueTotal = 0;
                MaxYearlyRevenue = 0;
                return;
            }

            double yearlyTotal = 0;
            double yearlyMax = 0;

            foreach (var dataPoint in yearlyData)
            {
                YearlyRevenue.Add(new RevenueDataPoint
                {
                    Label = dataPoint.Label,
                    Amount = dataPoint.Amount,
                    Timestamp = dataPoint.Timestamp
                });

                yearlyTotal += dataPoint.Amount;
                yearlyMax = Math.Max(yearlyMax, dataPoint.Amount);
            }

            YearlyRevenueTotal = yearlyTotal;
            MaxYearlyRevenue = yearlyMax;
        }

        private static PlotModel CreateRevenuePlotModel(IReadOnlyList<RevenueDataPoint> data)
        {
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                TextColor = OxyColor.FromRgb(66, 66, 66),
                PlotAreaBorderColor = OxyColor.FromArgb(0, 0, 0, 0)
            };

            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Angle = -45,
                GapWidth = 0.3,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                MinorStep = 1
            };

            foreach (var label in data.Select(d => d.Label))
            {
                xAxis.Labels.Add(label);
            }

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MinimumPadding = 0,
                MaximumPadding = 0.1,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 158, 158, 158),
                MinorGridlineStyle = LineStyle.None,
                StringFormat = "N0"
            };

            var series = new LineSeries
            {
                StrokeThickness = 2.5,
                Color = OxyColor.FromRgb(0, 150, 255),
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColor.FromRgb(0, 150, 255),
                CanTrackerInterpolatePoints = false
            };

            for (var i = 0; i < data.Count; i++)
            {
                series.Points.Add(new DataPoint(i, data[i].Amount));
            }

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(series);

            return model;
        }

        private static PlotModel CreateEmptyRevenuePlotModel()
        {
            var model = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
                TextColor = OxyColor.FromRgb(158, 158, 158)
            };

            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            xAxis.Labels.Add("Không có dữ liệu");

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 1,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None
            };

            var placeholderSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(189, 189, 189),
                StrokeThickness = 2
            };
            placeholderSeries.Points.Add(new DataPoint(0, 0));

            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(placeholderSeries);

            return model;
        }

        private void SyncAmenitiesFromHotel()
        {
            _isSyncingAmenities = true;
            HasFreeWifi = IsAmenitySelected("Free WiFi");
            HasSwimmingPool = IsAmenitySelected("Swimming Pool", "Pool");
            HasFreeParking = IsAmenitySelected("Free Parking");
            HasRestaurant = IsAmenitySelected("Restaurant");
            HasGym = IsAmenitySelected("Gym", "Fitness Center");
            _isSyncingAmenities = false;
        }

        private bool IsAmenitySelected(string amenityName, params string[] aliases)
        {
            if (CurrentHotel?.Amenities == null)
            {
                return false;
            }

            return CurrentHotel.Amenities.Any(a => MatchesAmenityName(a.AmenityName, amenityName, aliases));
        }

        private static bool MatchesAmenityName(string? value, string amenityName, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (string.Equals(value, amenityName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var alias in aliases)
            {
                if (string.Equals(value, alias, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateHotelAmenity(string amenityName, bool isSelected, params string[] aliases)
        {
            if (CurrentHotel == null)
            {
                return;
            }

            CurrentHotel.Amenities ??= new List<Amenity>();

            var existing = CurrentHotel.Amenities.FirstOrDefault(a => MatchesAmenityName(a.AmenityName, amenityName, aliases));

            if (isSelected)
            {
                if (existing == null)
                {
                    CurrentHotel.Amenities.Add(new Amenity
                    {
                        AmenityName = amenityName
                    });
                }
                else
                {
                    existing.AmenityName = amenityName;
                }
            }
            else if (existing != null)
            {
                CurrentHotel.Amenities.Remove(existing);
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

        [RelayCommand]
        private async Task UploadImage()
        {
            FileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";

            if (openFileDialog.ShowDialog() == true)
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
                {
                    return;
                }

                CurrentUser.AvatarUrl = await UploadImageService.UploadAsync(openFileDialog.FileName);
                await _userRepository.UpdateAsync(CurrentUser);
                if (!string.IsNullOrEmpty(CurrentUser.UserID))
                {
                    CurrentUser = await _userRepository.GetByIdAsync(CurrentUser.UserID) ?? CurrentUser;
                }

                ShowNotification("Profile image updated successfully.");
            }
        }

        [RelayCommand]
        private async Task UpdateInfo()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                return;
            }

            await _userRepository.UpdateAsync(CurrentUser);
            CurrentUser = await _userRepository.GetByIdAsync(CurrentUser.UserID) ?? CurrentUser;
            ShowNotification("Profile updated successfully.");
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.UserID))
            {
                return;
            }

            if (!_authenticationService.VerifyPassword(CurrentPassword, CurrentUser.Password))
            {
                ShowNotification("Current password is incorrect.");
                return;
            }

            if (!IsPasswordValid(NewPassword))
            {
                ShowNotification("New password does not meet requirements.");
                return;
            }

            if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                ShowNotification("New password and confirmation do not match.");
                return;
            }

            CurrentUser.Password = _authenticationService.HashPassword(NewPassword);
            await _userRepository.UpdateAsync(CurrentUser);
            CurrentUser = await _userRepository.GetByIdAsync(CurrentUser.UserID) ?? CurrentUser;

            ShowNotification("Password changed successfully.");
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        [RelayCommand]
        private void Logout()
        {
            _navigationService.NavigateToLogin();
        }

        [RelayCommand]
        private async Task NewHotel()
        {
            var dialog = App.Provider!.GetRequiredService<AddHotelDialog>();
            var hotel = new Hotel();
            dialog.DataContext = hotel;
            if (dialog.ShowDialog() == true)
            {
                hotel.HotelID = Guid.NewGuid().ToString();
                hotel.UserID = CurrentUser.UserID;
                hotel.IsApproved = false;
                hotel.IsVisible = true;
                await _hotelRepository.AddAsync(hotel);
                await _hotelRepository.SaveAsync();
                LoadHotels();
            }
        }

        [RelayCommand]
        private async Task UpdateHotelInfo()
        {
            if (CurrentHotel == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentHotel.HotelName) ||
                string.IsNullOrWhiteSpace(CurrentHotel.Address) ||
                string.IsNullOrWhiteSpace(CurrentHotel.City) ||
                CurrentHotel.Rating < 1 || CurrentHotel.Rating > 5)
            {
                return;
            }

            if (string.IsNullOrEmpty(CurrentHotel.HotelID))
            {
                CurrentHotel.HotelID = Guid.NewGuid().ToString();
                CurrentHotel.UserID = CurrentUser.UserID;
                CurrentHotel.IsApproved = false;
                CurrentHotel.IsVisible = true;
                await _hotelRepository.AddAsync(CurrentHotel);
                await _hotelRepository.SaveAsync();
                LoadHotels();
            }
            else
            {
                EnsureCityInOptions(CurrentHotel.City);
                await _hotelRepository.UpdateAsync(CurrentHotel);
            }
        }

        [RelayCommand]
        private async Task AddRoom()
        {
            if (CurrentHotel == null)
                return;
            var dialog = App.Provider!.GetRequiredService<AddRoomDialog>();
            var room = new Room
            {
                RoomID = Guid.NewGuid().ToString(),
                HotelID = CurrentHotel.HotelID,
                Status = "Available"
            };
            dialog.DataContext = room;
            if (dialog.ShowDialog() == true)
            {
                await _roomRepository.AddAsync(room);
                await _roomRepository.SaveAsync();
                await LoadRoomsAsync();
            }
        }

        [RelayCommand]
        private async Task EditRoom(Room? room)
        {
            if (room == null)
                return;

            var dialog = App.Provider!.GetRequiredService<EditRoomDialog>();
            var copy = new Room
            {
                RoomID = room.RoomID,
                HotelID = room.HotelID,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                RoomImage = room.RoomImage,
                Status = room.Status
            };
            dialog.DataContext = copy;
            if (dialog.ShowDialog() == true)
            {
                room.RoomNumber = copy.RoomNumber;
                room.RoomType = copy.RoomType;
                room.Capacity = copy.Capacity;
                room.PricePerNight = copy.PricePerNight;
                room.RoomImage = copy.RoomImage;
                room.Status = copy.Status;
                await _roomRepository.UpdateAsync(room);
                await LoadRoomsAsync();
                await LoadBookingsAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteHotel()
        {
            if (CurrentHotel == null)
            {
                return;
            }

            var hotelName = string.IsNullOrWhiteSpace(CurrentHotel.HotelName)
                ? "khách sạn này"
                : $"khách sạn \"{CurrentHotel.HotelName}\"";

            var confirmation = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa {hotelName}? Thao tác này sẽ xóa tất cả phòng, đặt phòng, thanh toán và đánh giá liên quan.",
                "Xác nhận xóa khách sạn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            var hotelId = CurrentHotel.HotelID;

            CurrentHotel = null;

            var rooms = await _roomRepository.GetAllAsync();
            var hotelRooms = rooms
                .Where(r => r.HotelID == hotelId)
                .ToList();

            foreach (var room in hotelRooms)
            {
                await _roomRepository.DeleteAsync(room.RoomID);
            }
            await _roomRepository.SaveAsync();

            var allBookings = await _bookingRepository.GetAllAsync();
            var hotelBookings = allBookings
                .Where(b => b.HotelID == hotelId)
                .ToList();

            var bookingIds = hotelBookings
                .Select(b => b.BookingID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            if (bookingIds.Count > 0)
            {
                var payments = await _paymentRepository.GetAllAsync();
                var relatedPayments = payments
                    .Where(p => !string.IsNullOrWhiteSpace(p.BookingID) && bookingIds.Contains(p.BookingID))
                    .ToList();

                foreach (var payment in relatedPayments)
                {
                    await _paymentRepository.DeleteAsync(payment.PaymentID);
                }
                await _paymentRepository.SaveAsync();
            }

            var reviews = await _reviewRepository.GetAllAsync();
            var hotelReviews = reviews
                .Where(r => r.HotelID == hotelId)
                .ToList();

            foreach (var review in hotelReviews)
            {
                await _reviewRepository.DeleteAsync(review.ReviewID);
            }
            await _reviewRepository.SaveAsync();

            foreach (var booking in hotelBookings)
            {
                await _bookingRepository.DeleteAsync(booking.BookingID);
            }
            await _bookingRepository.SaveAsync();

            await _hotelRepository.DeleteAsync(hotelId);
            await _hotelRepository.SaveAsync();

            LoadHotels();
            ShowNotification("Khách sạn đã được xóa thành công.");
        }

        [RelayCommand]
        private async Task UploadHotelImage()
        {
            if (CurrentHotel == null)
                return;

            FileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                CurrentHotel.HotelImage = await UploadImageService.UploadAsync(openFileDialog.FileName);
                await _hotelRepository.UpdateAsync(CurrentHotel);
                LoadHotels();
            }
        }

        [RelayCommand]
        private async Task RemoveRoom(Room? room)
        {
            if (room == null)
                return;

            var confirmationMessage = string.IsNullOrWhiteSpace(room.RoomNumber)
                ? "Bạn có chắc chắn muốn xóa phòng này?"
                : $"Bạn có chắc chắn muốn xóa phòng {room.RoomNumber}?";

            var confirmation = MessageBox.Show(
                confirmationMessage,
                "Xác nhận xóa phòng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            await _roomRepository.DeleteAsync(room.RoomID);
            await _roomRepository.SaveAsync();
            await LoadRoomsAsync();
            await LoadBookingsAsync();
        }

        [RelayCommand]
        private async Task ConfirmBooking(Booking? booking)
        {
            if (booking == null)
                return;

            if (booking.Status == "Pending")
            {
                booking.Status = "Confirmed";
                await _bookingRepository.UpdateAsync(booking);
                await LoadBookingsAsync();
            }
        }

        [RelayCommand]
        private async Task CancelBooking(Booking? booking)
        {
            if (booking == null)
                return;

            if (booking.Status == "Pending" || booking.Status == "CancelledRequested")
            {
                booking.Status = "Cancelled";
                await _bookingRepository.UpdateAsync(booking);
                await LoadBookingsAsync();
            }
        }

        [RelayCommand]
        private async Task RefreshBookings()
        {
            await LoadBookingsAsync();
        }

        private void EnsureCityInOptions(string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return;
            }

            if (!CityOptions.Contains(city))
            {
                CityOptions.Add(city);
            }
        }
    }

    public class RevenueDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RevenueFilterOption
    {
        public string DisplayName { get; set; } = string.Empty;
        public RevenueRange Range { get; set; }
    }

    public enum RevenueRange
    {
        Weekly,
        Monthly,
        Yearly,
        Cumulative
    }
}

