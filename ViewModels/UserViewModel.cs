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
        private readonly IHotelAdminRequestRepository _hotelAdminRequestRepository;
        private readonly IAuthentication _authenticationSerivce;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly INavigationService _navigationService;
        private readonly IBookingRepository _bookingRepository;

        private string userMail;
        private string _sortType = "Default";
        private string _showAvailableHotels = "Visible";
        private string _showRooms = "Collapsed";
        private string _showRegisterForm = "Collapsed";
        private string _showChatBox = "Collapsed";
        private string _showChatButton = "Visible";
        private Hotel _currentHotel;
        private User _currentUser;
        

        public string SortType { 
            get => _sortType; 
            set 
            {
                Set(ref _sortType, value);
                SortHotels();
            }
        }
        public User CurrentUser { get => _currentUser; set => Set(ref _currentUser, value); }
        public Hotel CurrentHotel { get => _currentHotel; set => Set(ref _currentHotel, value); }

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


        public ObservableCollection<Booking> Bookings { get; set; }

        public ObservableCollection<Hotel> Hotels
        {
            get;
            set;
        }

        public ObservableCollection<Room> Rooms
        {
            get;
            set;
        }

        public ObservableCollection<Room> FilteredRooms
        {
            get;
            set;
        }

        public ObservableCollection<AIChat> Chats { get; set; } = new ObservableCollection<AIChat>();

       
        
        public UserViewModel(IBookingRepository bookingRepository ,IUserRepository userRepository, IHotelRepository hotelRepository, INavigationService navigationService, IRoomRepository roomRepository, IAuthentication authentication, IHotelAdminRequestRepository hotelAdminRequestRepository)
        {
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
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




            Hotels = new ObservableCollection<Hotel>(_hotelRepository.GetAllAsync().Result);
            Rooms = new ObservableCollection<Room>(_roomRepository.GetAllAsync().Result);
            Bookings = new ObservableCollection<Booking>(_bookingRepository.GetAllAsync().Result);


            FilteredRooms = new ObservableCollection<Room>();

            GenerateData();

        }
        private void SortHotels()
        {
            if (SortType == "Rating: High to Low")
            {
                var sorted = Hotels.OrderByDescending(h => h.Rating).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            else if (SortType == "Rating: Low to High")
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
            else if (SortType == "Name: Z to A")
            {
                var sorted = Hotels.OrderByDescending(h => h.HotelName).ToList();
                Hotels.Clear();
                foreach (var hotel in sorted)
                {
                    Hotels.Add(hotel);
                }
            }
            //else
            //{
            //    Hotels = new ObservableCollection<Hotel>(_hotelRepository.GetAllAsync().Result);
            //}
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
        private void Send(string message)
        {
            Chats.Add(new AIChat { Message = message , Response = "Test"});
        }
// Hotel
//https://i.ibb.co/JWDk4mxz/download-2.jpg
//https://i.ibb.co/Z6XwmzzY/download-1.jpg
//https://i.ibb.co/Wp5NCf4k/download.jpg
//https://i.ibb.co/Ngw8W2PZ/hotel-photography-chup-anh-khach-san-khach-san-bamboo-sapa-hotel-18-1024x683.jpg

        //Room
//   https://i.ibb.co/ksLcW4k4/room4.jpg
//https://i.ibb.co/xKf2wdCn/room2.jpg
//https://i.ibb.co/TMyV0ngc/room1.jpg
        private void GenerateData()
        {
            Hotels.Add(new Hotel { MinPrice = 500, MaxPrice = 2000, HotelID = "HT1", HotelImage = "https://i.ibb.co/Ngw8W2PZ/hotel-photography-chup-anh-khach-san-khach-san-bamboo-sapa-hotel-18-1024x683.jpg", HotelName = "Hotel Sunshine", City = "Hà Nội", Rating = 5, Description = "A luxurious hotel in the heart of the city." });
            Hotels.Add(new Hotel {MinPrice = 200, MaxPrice = 700, HotelID = "HT2", HotelImage = "https://i.ibb.co/Z6XwmzzY/download-1.jpg", HotelName = "Ocean View Resort", City = "Đà Nẵng", Rating = 4, Description = "A beautiful resort with stunning ocean views." });
            Hotels.Add(new Hotel { MinPrice = 100, MaxPrice = 500, HotelImage = "https://i.ibb.co/JWDk4mxz/download-2.jpg", HotelID = "HT3", HotelName = "Mountain Retreat", City = "Đà Lạt", Rating = 3, Description = "A peaceful retreat in the mountains." });

            Rooms.Add(new Room {RoomImage= "https://i.ibb.co/ksLcW4k4/room4.jpg", RoomID = "R1", HotelID = "HT1", RoomNumber = "101", RoomType = "Single", PricePerNight = 100, Status = "Available" });
            Rooms.Add(new Room { RoomImage = "https://i.ibb.co/xKf2wdCn/room2.jpg", RoomID = "R2", HotelID = "HT1", RoomNumber = "102", RoomType = "Double", PricePerNight = 150, Status = "Available" });
            Rooms.Add(new Room { RoomImage = "https://i.ibb.co/TMyV0ngc/room1.jpg", RoomID = "R3", HotelID = "HT2", RoomNumber = "201", RoomType = "Suite", PricePerNight = 300, Status = "Booked" });
            Rooms.Add(new Room { RoomImage= "https://i.ibb.co/TMyV0ngc/room4.jpg", RoomID = "R4", HotelID = "HT2", RoomNumber = "202", RoomType = "Single", PricePerNight = 120, Status = "Available" });
            Rooms.Add(new Room { RoomImage = "https://i.ibb.co/TMyV0ngc/room2.jpg", RoomID = "R5", HotelID = "HT3", RoomNumber = "301", RoomType = "Double", PricePerNight = 180, Status = "Available" });
            Rooms.Add(new Room { RoomImage= "https://i.ibb.co/TMyV0ngc/room1.jpg", RoomID = "R6", HotelID = "HT3", RoomNumber = "302", RoomType = "Suite", PricePerNight = 350, Status = "Booked" });

            Bookings.Add(new Booking { BookingID = "B1", UserID = "U1", RoomID = "R3", CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(2), Status = "Confirmed" });
            Bookings.Add(new Booking { BookingID = "B2", UserID = "U2", RoomID = "R6", CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(3), Status = "Pending" });
            Bookings.Add(new Booking { BookingID = "B3", UserID = "U1", RoomID = "R2", CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(1), Status = "Cancelled" });
            Bookings.Add(new Booking { BookingID = "B4", UserID = "U3", RoomID = "R5", CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(4), Status = "Confirmed" });
        }

        [RelayCommand]
        private void ShowHotelDetails(string hotelID)
        {

            CurrentHotel = Hotels.FirstOrDefault(h => h.HotelID == hotelID);
            FilterRoomsByHotel(hotelID);
            ShowAvailableHotels = "Collapsed";
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
        private void HideRooms()
        {
            ShowRoomList = "Collapsed";
            ShowAvailableHotels = "Visible";
        }
        [RelayCommand]
        private void UpdateInfo()
        {
            _userRepository.UpdateAsync(CurrentUser);
           
        }
        [RelayCommand]
        private void Logout()
        {
            
            _navigationService.NavigateToLogin();
        }
        [RelayCommand]
        private void BookRoom(Room room)
        {
            MessageBox.Show($"Booking room {room.RoomNumber} - {room.RoomType} for ${room.PricePerNight}/night");
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

    }
}
