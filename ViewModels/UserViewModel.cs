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
        private readonly IAIChatRepository _aiChatRepository;
        private readonly IReviewRepository _reviewRepository;

        private string userMail;
        private int _totalBookings;
        private double _totalSpent;
        private string _showSearchRoom = "Collapsed";
        private string _showSearchHotel = "Visible";
        private string _sortType = "Default";
        private string _showAvailableHotels = "Visible";
        private string _showRooms = "Collapsed";
        private string _showRegisterForm = "Collapsed";
        private string _showChatBox = "Collapsed";
        private string _showChatButton = "Visible";
        private Hotel _currentHotel;
        private User _currentUser;

        public string ShowSearchRoom { get => _showSearchRoom; set => Set(ref _showSearchRoom, value); }
        public string ShowSearchHotel { get => _showSearchHotel; set => Set(ref _showSearchHotel, value); }
        public double TotalSpent { get => _totalSpent; set => Set(ref _totalSpent, value); }

        public int TotalBookings { get => _totalBookings; set => Set(ref _totalBookings, value); }

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

        public ObservableCollection<Booking> Bookings { get; set; } = new ObservableCollection<Booking>();

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
        public ObservableCollection<Review> Reviews { get; set; } = new ObservableCollection<Review>();


        public UserViewModel(IAIChatRepository aIChatRepository , IBookingRepository bookingRepository ,IUserRepository userRepository, IHotelRepository hotelRepository, INavigationService navigationService, IRoomRepository roomRepository, IAuthentication authentication, IHotelAdminRequestRepository hotelAdminRequestRepository, IReviewRepository reviewRepository)
        {

            _aiChatRepository = aIChatRepository;
            _hotelAdminRequestRepository = hotelAdminRequestRepository;
            _bookingRepository = bookingRepository;
            _authenticationSerivce = authentication;
            _navigationService = navigationService;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;

            WeakReferenceMessenger.Default.Register<UserViewModel, MessageService>(
     this,
     (recipient, message) =>
     {
         recipient.userMail = message.Value;
         GetCurrentUser();
         FilterBookingsByUser(CurrentUser.UserID);
     });


          //  SeedData();

            Hotels = new ObservableCollection<Hotel>(_hotelRepository.GetAllAsync().Result);
            Rooms = new ObservableCollection<Room>(_roomRepository.GetAllAsync().Result);
            



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


                List<Hotel> hotels = _hotelRepository.GetAllAsync().Result;



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
        private void ClearSearch()
        {
            Hotels.Clear();
            var allHotels = _hotelRepository.GetAllAsync().Result;
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
        private void Send(string message)
        {
            Chats.Add(new AIChat { Message = message , Response = "Test"});
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
                CurrentUser = await _userRepository.GetByIdAsync(CurrentUser.UserID);
            }
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
            var hotel = Hotels.FirstOrDefault(h => h.HotelID == room.HotelID);
            bool res = _navigationService.OpenBookingDialog(room, CurrentUser, hotel);

            if (res)
            {
                FilterRoomsByHotel(room.HotelID);
                FilterBookingsByUser(CurrentUser.UserID);
            }
        }
        [RelayCommand]
        private void ReviewBooking(Booking booking)
        {
            bool res = _navigationService.OpenReviewDialog(booking);
            if (res)
            {
                LoadReviewsForHotel(booking.HotelID);
            }
        }
        private void FilterBookingsByUser(string userId)
        {
            var bookingList = _bookingRepository.GetBookingByUserId(userId).Result;
            Bookings.Clear();
            var userBookings = bookingList.Where(b => b.UserID == userId).ToList();
            foreach (var booking in userBookings)
            {
                Bookings.Add(booking);
                //TODO: Calculate total spent
            }


            TotalBookings = Bookings.Count;
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
        private void LoadReviewsForHotel(string hotelId)
        {
            Reviews.Clear();
            var reviewList = _reviewRepository.GetAllAsync().Result;
            var hotelReviews = reviewList.Where(r => r.HotelID == hotelId).ToList();
            foreach (var review in hotelReviews)
            {
                Reviews.Add(review);
            }
        }

        
        

    }
}
