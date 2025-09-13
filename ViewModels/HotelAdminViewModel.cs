using System.Collections.ObjectModel;
using System.Linq;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class HotelAdminViewModel : Bindable, IHotelAdminViewModel
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;

        private Hotel? _currentHotel;
        public Hotel? CurrentHotel { get => _currentHotel; set => Set(ref _currentHotel, value); }

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Booking> Bookings { get; } = new();
        public ObservableCollection<Review> Reviews { get; } = new();

        public HotelAdminViewModel(
            IRoomRepository roomRepository,
            IHotelRepository hotelRepository,
            IBookingRepository bookingRepository,
            IReviewRepository reviewRepository)
        {
            _roomRepository = roomRepository;
            _hotelRepository = hotelRepository;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;

            LoadHotel();
            LoadRooms();
            LoadBookings();
            LoadReviews();
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

        private void LoadReviews()
        {
            var reviews = _reviewRepository.GetAllAsync().Result;
            Reviews.Clear();
            foreach (var review in reviews)
            {
                Reviews.Add(review);
            }
        }
    }
}
