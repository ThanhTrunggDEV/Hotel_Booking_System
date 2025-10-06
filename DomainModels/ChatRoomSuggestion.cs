using System.Globalization;

namespace Hotel_Booking_System.DomainModels
{
    public class ChatRoomSuggestion
    {
        public required Hotel Hotel { get; init; }
        public required Room Room { get; init; }
        public double? UserRating { get; init; }

        public string DisplayTitle => $"{Hotel.HotelName} - Phòng {Room.RoomNumber}";

        public string DisplayLocation => string.IsNullOrWhiteSpace(Hotel.Address)
            ? Hotel.City
            : $"{Hotel.Address}, {Hotel.City}".Trim(',', ' ');

        public string DisplaySubtitle => $"{Room.RoomType} • tối đa {Room.Capacity} khách • {Room.PricePerNight.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"))}/đêm";

        public string RatingText
        {
            get
            {
                if (UserRating.HasValue)
                {
                    return $"Đánh giá khách: {UserRating.Value:F1}/5";
                }

                return $"Hạng hệ thống: {Hotel.Rating}/5";
            }
        }
    }
}
