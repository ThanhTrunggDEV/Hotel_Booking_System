using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
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
    public partial class UserViewModel : INotifyPropertyChanged
    {
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        
        private string userMail;
        private Hotel _selectedHotel;
        private bool _showRooms = false;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public Hotel SelectedHotel
        {
            get => _selectedHotel;
            set
            {
                _selectedHotel = value;
                OnPropertyChanged(nameof(SelectedHotel));
            }
        }

        public bool ShowRooms
        {
            get => _showRooms;
            set
            {
                _showRooms = value;
                OnPropertyChanged(nameof(ShowRooms));
            }
        }

        public UserViewModel()
        {
            WeakReferenceMessenger.Default.Register<MessageService>(this, (r, msg) =>
            {
                userMail = msg.Value;
            });

            LoadHotels();
            LoadChats();
            LoadRooms();
        }

        [RelayCommand]
        private void Send(string message)
        {
            Chats.Add(new AIChat { Message = message });
        }

        private void LoadChats()
        {
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello how can I help?" });
            Chats.Add(new AIChat { Message = "Hello", Response = "Hello             gfddddddddddddddddddddddddddddddddddddddddddddd  fdgg fdhow can I help?" });
            Chats.Add(new AIChat { Message = "Hello What is the thgoeuihgjuerigyhergergergdfgfdgfdgdddddddddddddddgggggggggggggggggregregergergergergregergergdfgdgdgdfgregregreg", Response = "Hello how can I help?" });
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
            
           
            Rooms.Add(new Room { RoomID = "4", HotelID = "2", RoomNumber = "101", RoomType = "Boutique Room", PricePerNight = 85.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "5", HotelID = "2", RoomNumber = "102", RoomType = "Garden View", PricePerNight = 95.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            
            
            Rooms.Add(new Room { RoomID = "6", HotelID = "3", RoomNumber = "101", RoomType = "Business Room", PricePerNight = 95.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "7", HotelID = "3", RoomNumber = "102", RoomType = "Executive Suite", PricePerNight = 150.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
            Rooms.Add(new Room { RoomID = "8", HotelID = "3", RoomNumber = "103", RoomType = "Standard Room", PricePerNight = 75.0, Status = "Available", RoomImage = "https://i.ibb.co/0Rxmv6B9/444503660-122160335006059468-7985090248807237237-n.jpg" });
        }

        [RelayCommand]
        private void ShowHotelDetails(Hotel hotel)
        {
            SelectedHotel = hotel;
            FilterRoomsByHotel(hotel.HotelID);
            ShowRooms = true;
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
            ShowRooms = false;
            SelectedHotel = null;
        }

        [RelayCommand]
        private void BookRoom(Room room)
        {
            MessageBox.Show($"Booking room {room.RoomNumber} - {room.RoomType} for ${room.PricePerNight}/night");
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
