using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static System.Net.WebRequestMethods;

namespace Hotel_Booking_System.ViewModels
{
    public partial class UserViewModel : Bindable, IUserViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthentication _authenticationSerivce;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly INavigationService _navigationService;
        private readonly IBookingRepository _bookingRepository;

        private string userMail;
        private string _showAvailableHotels = "Visible";
        private string _showRooms = "Collapsed";
        private Hotel _currentHotel;
        private User _currentUser;
        
        public User CurrentUser { get => _currentUser; set => Set(ref _currentUser, value); }
        public Hotel CurrentHotel { get => _currentHotel; set => Set(ref _currentHotel, value); }

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


        public ObservableCollection<Booking> Bookings { get; set; } = new ObservableCollection<Booking>();

        public ObservableCollection<Hotel> Hotels
        {
            get;
            set;
        } = new ObservableCollection<Hotel>();

        public ObservableCollection<Room> Rooms
        {
            get;
            set;
        } = new ObservableCollection<Room>();

        public ObservableCollection<Room> FilteredRooms
        {
            get;
            set;
        } = new ObservableCollection<Room>();

        public ObservableCollection<AIChat> Chats { get; set; } = new ObservableCollection<AIChat>();

       
        private void LoadBookings()
        {
            
            Bookings.Add(new Booking { BookingID = "B001", UserID = "U001", RoomID = "R101", CheckInDate = DateTime.Now.AddDays(1), CheckOutDate = DateTime.Now.AddDays(5), Status = "Confirmed" });
            Bookings.Add(new Booking { BookingID = "B002", UserID = "U002", RoomID = "R102", CheckInDate = DateTime.Now.AddDays(3), CheckOutDate = DateTime.Now.AddDays(6),  Status = "Pending" });
            Bookings.Add(new Booking { BookingID = "B003", UserID = "U003", RoomID = "R201", CheckInDate = DateTime.Now.AddDays(2), CheckOutDate = DateTime.Now.AddDays(4), Status = "Cancelled" });
        }
        public UserViewModel(IBookingRepository bookingRepository ,IUserRepository userRepository, IHotelRepository hotelRepository, INavigationService navigationService, IRoomRepository roomRepository, IAuthentication authentication)
        {
            _bookingRepository = bookingRepository;
            _authenticationSerivce = authentication;
            _navigationService = navigationService;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;

            WeakReferenceMessenger.Default.Register<UserViewModel, MessageService>(
     this,
     (recipient, message) =>
     {
         recipient.userMail = message.Value;
         GetCurrentUser();
     });




            LoadHotels();
            LoadChats();
            LoadBookings();
            LoadRooms();
           
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
        private void Send(string message)
        {
            Chats.Add(new AIChat { Message = message , Response = "Test"});
        }

        private void LoadChats()
        {
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            
        }

        private void LoadHotels()
        {
            Hotels.Add(new Hotel { HotelID = "1", HotelName = "Luxury Downtown Hotel", Address = "123 Main Street", City = "Hanoi", Description = "Experience luxury in the heart of the city with stunning views and world-class amenities.", Rating = 5, HotelImage = @"D:\Wallpaper\IMG_7230.JPG" });
            Hotels.Add(new Hotel { HotelID = "2", HotelName = "Historic Boutique Hotel", Address = "456 Old Quarter", City = "Hanoi", Description = "A charming boutique hotel in the historic district with authentic Vietnamese architecture.", Rating = 4, HotelImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Hotels.Add(new Hotel { HotelID = "3", HotelName = "Modern City Hotel", Address = "789 Business District", City = "Hanoi", Description = "Contemporary gdfhgf jgfdn gjkdf ndfjkg hfd gfjkd hfd ghfdjk ghdfjk hgdfj ghfdkj hgdfjk ghfdjk ghdfkj ghfdk jgfd g design meets comfort in this modern hotel perfect for business travelers.", Rating = 4, HotelImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Hotels.Add(new Hotel { HotelID = "4", HotelName = "Riverside Resort", Address = "321 River Road", City = "Hanoi", Description = "Peaceful riverside location with beautiful gardens and outdoor swimming pool.", Rating = 5, HotelImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Hotels.Add(new Hotel { HotelID = "5", HotelName = "Airport Hotel", Address = "654 Airport Boulevard", City = "Hanoi", Description = "Convenient location near the airport with shuttle service and comfortable rooms.", Rating = 3, HotelImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
        }

        private void LoadRooms()
        {
            
            Rooms.Add(new Room { RoomID = "1", HotelID = "1", RoomNumber = "101", RoomType = "Deluxe King", PricePerNight = 120.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "2", HotelID = "1", RoomNumber = "102", RoomType = "Deluxe Twin", PricePerNight = 110.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });

            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "3", HotelID = "1", RoomNumber = "201", RoomType = "Suite", PricePerNight = 200.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });

            Rooms.Add(new Room { RoomID = "4", HotelID = "2", RoomNumber = "101", RoomType = "Boutique Room", PricePerNight = 85.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "5", HotelID = "2", RoomNumber = "102", RoomType = "Garden View", PricePerNight = 95.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            
            
            Rooms.Add(new Room { RoomID = "6", HotelID = "3", RoomNumber = "101", RoomType = "Business Room", PricePerNight = 95.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "7", HotelID = "3", RoomNumber = "102", RoomType = "Executive Suite", PricePerNight = 150.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "8", HotelID = "3", RoomNumber = "103", RoomType = "Standard Room", PricePerNight = 75.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
        }

        [RelayCommand]
        private void ShowHotelDetails(string hotelID)
        {

            CurrentHotel = Hotels.FirstOrDefault(h => h.HotelID == hotelID);
            FilterRoomsByHotel(hotelID);
            ShowAvailableHotels = "Collapsed";
            ShowRoomList = "Visible";
        }

        private void FilterRoomsByHotel(string hotelId)
        {
            FilteredRooms.Clear();
            var hotelRooms = Rooms.Where(r => r.HotelID == hotelId).ToList();
            foreach (var room in hotelRooms)
            {
                FilteredRooms.Add(room);
            }
        }

        [RelayCommand]
        private void HideRooms()
        {
            ShowRoomList = "Collapsed";
            ShowAvailableHotels = "Visible";
        }

        [RelayCommand]
        private void BookRoom(Room room)
        {
            MessageBox.Show($"Booking room {room.RoomNumber} - {room.RoomType} for ${room.PricePerNight}/night");
        }

        
    }
}
