using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    /// <summary>
    /// View model used by the <see cref="Views.HotelAdminWindow"/>.  It exposes
    /// a command allowing the administrator to persist changes to the hotel
    /// information.  The command performs some very light validation before
    /// delegating to <see cref="IHotelRepository"/> for persistence.
    /// </summary>
    public partial class HotelAdminViewModel : Bindable
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IReviewRepository _reviewRepository;

        public ObservableCollection<Review> Reviews { get; } = new();

        private Hotel _hotel = new();
        public Hotel Hotel
        {
            get => _hotel;
            set => Set(ref _hotel, value);
        }

        public HotelAdminViewModel(IHotelRepository hotelRepository,
                                   IReviewRepository reviewRepository)
        {
            _hotelRepository = hotelRepository;
            _reviewRepository = reviewRepository;
            LoadReviews();
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

        /// <summary>
        /// Command invoked from the UI when the admin wants to store the
        /// current hotel information.
        /// </summary>
        [RelayCommand]
        private async Task UpdateHotelInfo()
        {
            // Basic validation – ensure required fields are populated.
            if (string.IsNullOrWhiteSpace(Hotel.HotelName) ||
                string.IsNullOrWhiteSpace(Hotel.Address) ||
                Hotel.Rating < 1 || Hotel.Rating > 5)
            {
                return;
            }

            if (string.IsNullOrEmpty(Hotel.HotelID))
            {
                Hotel.HotelID = Guid.NewGuid().ToString();
                await _hotelRepository.AddAsync(Hotel);
                await _hotelRepository.SaveAsync();
            }
            else
            {
                await _hotelRepository.UpdateAsync(Hotel);
            }
        }
    }
}

