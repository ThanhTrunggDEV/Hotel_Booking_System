using System;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    public partial class ReviewViewModel : Bindable, IReviewViewModel
    {
        private readonly IReviewRepository _reviewRepository;

        private Booking _booking;
        public Booking Booking
        {
            get => _booking;
            set => Set(ref _booking, value);
        }

        private int _rating = 5;
        public int Rating
        {
            get => _rating;
            set => Set(ref _rating, value);
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set => Set(ref _comment, value);
        }

        public ReviewViewModel(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [RelayCommand]
        private async Task Submit()
        {
            if (Booking == null)
            {
                return;
            }

            var existingReviews = await _reviewRepository.GetAllAsync();
            if (existingReviews.Any(r => r.BookingID == Booking.BookingID))
            {
                MessageBox.Show("You have already submitted a review for this booking.",
                                "Review Exists", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var review = new Review
            {
                UserID = Booking.UserID,
                HotelID = Booking.HotelID,
                RoomID = Booking.RoomID,
                BookingID = Booking.BookingID,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveAsync();
        }
    }
}
