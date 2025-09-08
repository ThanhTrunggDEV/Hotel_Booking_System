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
        private void SearchHotels(object parameter)
        {
             
            if (parameter is Dictionary<string, object?> searchParams)
            {
                string location = searchParams.TryGetValue("Location", out var loc) ? loc?.ToString() ?? string.Empty : string.Empty;
                decimal? minPrice = searchParams.TryGetValue("MinPrice", out var min) && min is decimal dmin ? dmin : null;
                decimal? maxPrice = searchParams.TryGetValue("MaxPrice", out var max) && max is decimal dmax ? dmax : null;
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


                var hotels = Hotels .AsQueryable();



                if (!string.IsNullOrWhiteSpace(location))
                {
                    hotels = hotels.Where(h =>
                        (!string.IsNullOrEmpty(h.City) && h.City == location) ||
                        (!string.IsNullOrEmpty(h.HotelName) && h.HotelName.Contains(location, StringComparison.OrdinalIgnoreCase)));
                }

                
                if (minPrice.HasValue)
                    hotels = hotels.Where(h => h.MinPrice >= (double)minPrice.Value);
                if (maxPrice.HasValue)
                    hotels = hotels.Where(h => h.MaxPrice <= (double)maxPrice.Value);

                var starFilters = new List<int>();
                if (fiveStar == true) starFilters.Add(5);
                if (fourStar == true) starFilters.Add(4);
                if (threeStar == true) starFilters.Add(3);
                if (twoStar == true) starFilters.Add(2);
                if (oneStar == true) starFilters.Add(1);
                if (starFilters.Count > 0)
                    hotels = hotels.Where(h => starFilters.Contains(h.Rating));


                //var filteredHotels = hotels.ToList();
                //var currentHotel = Hotels.ToList();

                //foreach (var hotel in currentHotel)
                //{
                //    if (hotel.HotelID == )
                //}

                var filteredHotels = hotels.ToList();
                for (int i = Hotels.Count - 1; i >= 0; i--)
                {
                    if (!filteredHotels.Any(h => h.HotelID == Hotels[i].HotelID))
                    {
                        Hotels.RemoveAt(i);
                    }
                }

            }
            //else
            //{
            //    Hotels = new ObservableCollection<Hotel>(_hotelRepository.GetAllAsync().Result);
            //}
            
            
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
