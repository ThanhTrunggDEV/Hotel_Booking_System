using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Booking_System.ViewModels
{
    public class HotelReviewsViewModel : Bindable
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;

        private readonly ObservableCollection<string> _ratingFilters = new()
        {
            "Tất cả",
            "5 sao",
            "4 sao",
            "3 sao",
            "2 sao",
            "1 sao"
        };

        private List<Review> _allReviews = new();
        private string _selectedRatingFilter;
        private string _searchKeyword = string.Empty;
        private string _emptyStateVisibility = "Collapsed";
        private string _reviewListVisibility = "Collapsed";
        private string _summaryVisibility = "Collapsed";
        private string _hotelName = string.Empty;
        private double _averageRating;
        private int _totalReviews;

        public HotelReviewsViewModel(IReviewRepository reviewRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _selectedRatingFilter = _ratingFilters.First();
        }

        public ObservableCollection<Review> Reviews { get; } = new();

        public ObservableCollection<string> RatingFilters => _ratingFilters;

        public string SelectedRatingFilter
        {
            get => _selectedRatingFilter;
            set
            {
                if (_selectedRatingFilter == value)
                {
                    return;
                }

                Set(ref _selectedRatingFilter, value);
                ApplyFilters();
            }
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword == value)
                {
                    return;
                }

                Set(ref _searchKeyword, value);
                ApplyFilters();
            }
        }

        public string EmptyStateVisibility
        {
            get => _emptyStateVisibility;
            private set => Set(ref _emptyStateVisibility, value);
        }

        public string HotelName
        {
            get => _hotelName;
            private set => Set(ref _hotelName, value);
        }

        public string ReviewListVisibility
        {
            get => _reviewListVisibility;
            private set => Set(ref _reviewListVisibility, value);
        }

        public string SummaryVisibility
        {
            get => _summaryVisibility;
            private set => Set(ref _summaryVisibility, value);
        }

        public double AverageRating
        {
            get => _averageRating;
            private set => Set(ref _averageRating, value);
        }

        public int TotalReviews
        {
            get => _totalReviews;
            private set => Set(ref _totalReviews, value);
        }

        public ObservableCollection<RatingBreakdown> RatingBreakdowns { get; } = new();

        public async Task InitializeAsync(Hotel? hotel)
        {
            if (hotel == null)
            {
                return;
            }

            HotelName = hotel.HotelName;
            await LoadReviewsAsync(hotel.HotelID);
        }

        private async Task LoadReviewsAsync(string hotelId)
        {
            var reviews = await _reviewRepository.GetAllAsync();
            var hotelReviews = reviews
                .Where(r => r.HotelID == hotelId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            if (hotelReviews.Count == 0)
            {
                _allReviews = new List<Review>();
                ApplyFilters();
                return;
            }

            var userIds = hotelReviews
                .Select(r => r.UserID)
                .Distinct()
                .ToList();

            var userTasks = userIds
                .Select(id => _userRepository.GetByIdAsync(id));

            var users = await Task.WhenAll(userTasks);
            var userLookup = users
                .Where(u => u != null)
                .Cast<User>()
                .ToDictionary(u => u.UserID, u => u);

            foreach (var review in hotelReviews)
            {
                if (userLookup.TryGetValue(review.UserID, out var user))
                {
                    review.ReviewerName = user.FullName;
                    review.ReviewerAvatarUrl = user.AvatarUrl;
                }

                review.AdminReplyDraft = string.Empty;
            }

            _allReviews = hotelReviews;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            Reviews.Clear();

            IEnumerable<Review> filtered = _allReviews;

            var rating = GetRatingFromFilter();
            if (rating.HasValue)
            {
                filtered = filtered.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.Trim();
                filtered = filtered.Where(r =>
                    (!string.IsNullOrEmpty(r.Comment) && r.Comment.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.ReviewerName) && r.ReviewerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (var review in filtered)
            {
                Reviews.Add(review);
            }

            EmptyStateVisibility = Reviews.Count == 0 ? "Visible" : "Collapsed";
            ReviewListVisibility = Reviews.Count == 0 ? "Collapsed" : "Visible";
            UpdateSummary();
        }

        private int? GetRatingFromFilter()
        {
            if (string.IsNullOrWhiteSpace(SelectedRatingFilter) || SelectedRatingFilter.Equals(_ratingFilters.First(), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var digits = new string(SelectedRatingFilter.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var rating) && rating >= 1 && rating <= 5)
            {
                return rating;
            }

            return null;
        }

        private void UpdateSummary()
        {
            TotalReviews = _allReviews.Count;

            if (TotalReviews == 0)
            {
                SummaryVisibility = "Collapsed";
                AverageRating = 0;
                RatingBreakdowns.Clear();
                return;
            }

            AverageRating = Math.Round(_allReviews.Average(r => r.Rating), 1);

            RatingBreakdowns.Clear();
            for (var rating = 5; rating >= 1; rating--)
            {
                var count = _allReviews.Count(r => r.Rating == rating);
                var percentage = TotalReviews == 0 ? 0 : (double)count / TotalReviews;
                RatingBreakdowns.Add(new RatingBreakdown
                {
                    Rating = rating,
                    Count = count,
                    Percentage = percentage
                });
            }

            SummaryVisibility = "Visible";
        }

        public class RatingBreakdown
        {
            public int Rating { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
            public string DisplayLabel => $"{Rating} sao";
        }
    }
}
