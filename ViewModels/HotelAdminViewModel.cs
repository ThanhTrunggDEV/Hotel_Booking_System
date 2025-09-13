using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class HotelAdminViewModel : Bindable, IHotelAdminViewModel
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IReviewRepository _reviewRepository;

        public ObservableCollection<Room> Rooms { get; } = new();
        public ObservableCollection<Review> Reviews { get; } = new();

        public HotelAdminViewModel(IRoomRepository roomRepository, IReviewRepository reviewRepository)
        {
            _roomRepository = roomRepository;
            _reviewRepository = reviewRepository;
            LoadRooms();
            LoadReviews();
        }

        private async void LoadRooms()
        {
            var rooms = await _roomRepository.GetAllAsync();
            Rooms.Clear();
            foreach (var room in rooms)
            {
                Rooms.Add(room);
            }
        }

        private async void LoadReviews()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            Reviews.Clear();
            foreach (var review in reviews)
            {
                Reviews.Add(review);
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
