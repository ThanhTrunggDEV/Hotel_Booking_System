using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;

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

        public Hotel? CurrentHotel
        {
            get => _currentHotel;
            set => Set(ref _currentHotel, value);
        }

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Booking> Bookings { get; } = new();
        public ObservableCollection<Review> Reviews { get; } = new();

        public User CurrentUser
        {
            get => _currentUser;
            set => Set(ref _currentUser, value);
        }

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

            LoadHotel();
            LoadRooms();
            LoadBookings();
            LoadReviews();
        }

        private async void LoadCurrentUser()
        {
            CurrentUser = await _userRepository.GetByEmailAsync(_userEmail);
        }

        private void LoadHotel()
        {
            var hotels = _hotelRepository.GetAllAsync().Result;
            CurrentHotel = hotels.FirstOrDefault();
        }

        private void LoadRooms()
        {
            var rooms = _roomRepository.GetAllAsync().Result;
            Rooms.Clear();
            foreach (var room in rooms)
            {
                Rooms.Add(room);
            }
        }

        private void LoadBookings()
        {
            var bookings = _bookingRepository.GetAllAsync().Result;
            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }
        }

        private async void LoadReviews()
        {
            await LoadReviewsAsync();
        }

        public async Task LoadReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            Reviews.Clear();
            foreach (var review in reviews)
            {
                Reviews.Add(review);
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
                await _hotelRepository.AddAsync(CurrentHotel);
                await _hotelRepository.SaveAsync();
            }
            else
            {
                await _hotelRepository.UpdateAsync(CurrentHotel);
            }
        }

        [RelayCommand]
        private async Task AddRoom()
        {
            var room = new Room
            {
                RoomID = Guid.NewGuid().ToString(),
                Status = "Available"
            };
            await _roomRepository.AddAsync(room);
            await _roomRepository.SaveAsync();
            LoadRooms();
        }

        [RelayCommand]
        private async Task EditRoom(Room? room)
        {
            if (room == null)
                return;

            await _roomRepository.UpdateAsync(room);
            await _roomRepository.SaveAsync();
            LoadRooms();
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
    }
}

