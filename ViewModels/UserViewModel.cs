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
        private string _showAvailableHotels = "Visible";
        private string _showRooms = "Collapsed";
        private string _showRegisterForm = "Collapsed";
        private Hotel _currentHotel;
        private User _currentUser;
        
        public User CurrentUser { get => _currentUser; set => Set(ref _currentUser, value); }
        public Hotel CurrentHotel { get => _currentHotel; set => Set(ref _currentHotel, value); }

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

       
        private void LoadBookings()
        {
            
            Bookings.Add(new Booking { BookingID = "B001", UserID = "U001", RoomID = "R101", CheckInDate = DateTime.Now.AddDays(1), CheckOutDate = DateTime.Now.AddDays(5), Status = "Confirmed" });
            Bookings.Add(new Booking { BookingID = "B002", UserID = "U002", RoomID = "R102", CheckInDate = DateTime.Now.AddDays(3), CheckOutDate = DateTime.Now.AddDays(6),  Status = "Pending" });
            Bookings.Add(new Booking { BookingID = "B003", UserID = "U003", RoomID = "R201", CheckInDate = DateTime.Now.AddDays(2), CheckOutDate = DateTime.Now.AddDays(4), Status = "Cancelled" });
        }
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
