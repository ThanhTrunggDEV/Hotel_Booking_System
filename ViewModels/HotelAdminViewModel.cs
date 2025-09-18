using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.Views;
using Hotel_Manager.FrameWorks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Hotel_Booking_System.ViewModels
{
    public partial class HotelAdminViewModel : Bindable, IHotelAdminViewModel
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IBookingRepository _bookingRepository;

        private string _userEmail = string.Empty;
        private User _currentUser = new();
        private Hotel? _currentHotel;
        private bool _isSyncingAmenities;

        private bool _hasFreeWifi;
        private bool _hasSwimmingPool;
        private bool _hasFreeParking;
        private bool _hasRestaurant;
        private bool _hasGym;

        public ObservableCollection<Hotel> Hotels { get; } = new();
        public Hotel? CurrentHotel
        {
            get => _currentHotel;
            set
            {
                Set(ref _currentHotel, value);

                SyncAmenitiesFromHotel();
                LoadRooms();
                LoadBookings();
                LoadReviews();
            }
        }

        private readonly List<Booking> _allBookings = new();

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Booking> Bookings { get; } = new();
        public ObservableCollection<string> BookingStatusFilters { get; } = new();

        private string _selectedBookingStatusFilter = "All";
        public string SelectedBookingStatusFilter
        {
            get => _selectedBookingStatusFilter;
            set
            {
                if (Set(ref _selectedBookingStatusFilter, value))
                {
                    ApplyBookingFilter();
                }
            }
        }
        public ObservableCollection<Review> Reviews { get; } = new();

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
            private set => Set(ref _totalReviews, value);
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
            private set => Set(ref _fiveStarCount, value);
        }

        private int _fourStarCount;
        public int FourStarCount
        {
            get => _fourStarCount;
            private set => Set(ref _fourStarCount, value);
        }

        private int _threeStarCount;
        public int ThreeStarCount
        {
            get => _threeStarCount;
            private set => Set(ref _threeStarCount, value);
        }

        private int _twoStarCount;
        public int TwoStarCount
        {
            get => _twoStarCount;
            private set => Set(ref _twoStarCount, value);
        }

        private int _oneStarCount;
        public int OneStarCount
        {
            get => _oneStarCount;
            private set => Set(ref _oneStarCount, value);
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
            IBookingRepository bookingRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _roomRepository = roomRepository;
            _hotelRepository = hotelRepository;
            _bookingRepository = bookingRepository;

            WeakReferenceMessenger.Default.Register<HotelAdminViewModel, MessageService>(this, (recipient, message) =>
            {
                recipient._userEmail = message.Value;
                recipient.LoadCurrentUser();
            });
        }

        private async void LoadCurrentUser()
        {
            CurrentUser = await _userRepository.GetByEmailAsync(_userEmail);
            LoadHotels();
        }

        private async void LoadHotels()
        {
            if (string.IsNullOrEmpty(CurrentUser.UserID))
                return;

            var hotels = await _hotelRepository.GetAllAsync();
            Hotels.Clear();
            foreach (var hotel in hotels.Where(h => h.UserID == CurrentUser.UserID && h.IsApproved))
            {
                Hotels.Add(hotel);
            }

            CurrentHotel = Hotels.FirstOrDefault();
        }

        private void LoadRooms()
        {
            if (CurrentHotel == null)
                return;

            var rooms = _roomRepository.GetAllAsync().Result
                .Where(r => r.HotelID == CurrentHotel.HotelID);
            Rooms.Clear();
            foreach (var room in rooms)
            {
                Rooms.Add(room);
            }
        }

        private void LoadBookings()
        {
            if (CurrentHotel == null)
                return;

            var roomNumbers = _roomRepository.GetAllAsync().Result
                .Where(r => r.HotelID == CurrentHotel.HotelID)
                .ToDictionary(r => r.RoomID, r => r.RoomNumber);

            var bookings = _bookingRepository.GetAllAsync().Result
                .Where(b => b.HotelID == CurrentHotel.HotelID)
                .ToList();

            _allBookings.Clear();
            foreach (var booking in bookings)
            {
                booking.RoomNumber = roomNumbers.TryGetValue(booking.RoomID, out var number)
                    ? number
                    : booking.RoomID;
                _allBookings.Add(booking);
            }

            UpdateBookingStatusFilters();
            ApplyBookingFilter();
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
            Reviews.Clear();
            foreach (var review in reviews.Where(r => r.HotelID == CurrentHotel.HotelID))
            {
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

            foreach (var booking in filtered)
            {
                Bookings.Add(booking);
            }
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
                LoadRooms();
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
                await _roomRepository.UpdateAsync(room);
                LoadRooms();
            }
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

            await _roomRepository.DeleteAsync(room.RoomID);
            await _roomRepository.SaveAsync();
            LoadRooms();
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
                LoadBookings();
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
                LoadBookings();
            }
        }
    }
}

