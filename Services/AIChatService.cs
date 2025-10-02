using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Mscc.GenerativeAI;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;
        private readonly GeminiOptions _options;
        private readonly GenerativeModel _generativeModel;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;

        public AIChatService(IAIChatRepository repository, GeminiOptions options, IBookingRepository bookingRepository, IHotelRepository hotelRepository, IRoomRepository roomRepository, IReviewRepository reviewRepository)
        {
            _repository = repository;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
            _options = options;

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                         ?? throw new InvalidOperationException("GEMINI_API_KEY is not set");

            var modelName = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                            ?? _options.DefaultModel
                            ?? Model.Gemini15Pro; 

         
            var googleAI = new GoogleAI(apiKey);
            _generativeModel = googleAI.GenerativeModel(model: modelName);
        }

        public async Task<AIChat> SendAsync(string userId, string message, string? model = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            // 1. Lấy data từ DB
            var hotels = await _hotelRepository.GetAllAsync();
            var rooms = await _roomRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetBookingByUserId(userId);
            var reviews = await _reviewRepository.GetAllAsync();

            var ratingsByHotel = reviews
                .GroupBy(r => r.HotelID)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            // 2. Build context text
            var context = new StringBuilder();
            context.AppendLine("Here is the current hotel database snapshot:");
            context.AppendLine("\nHotels:");
            foreach (var h in hotels)
            {
                ratingsByHotel.TryGetValue(h.HotelID, out var userRating);
                context.AppendLine($"- {h.HotelID}, {h.HotelName}, Location: {h.Address}, {h.City}, Price Range: {h.MinPrice}-{h.MaxPrice}, Hotel Rating: {h.Rating}, User Rating: {userRating:F1}");
            }

            context.AppendLine("\nRooms:");
            foreach (var r in rooms)
            {
                context.AppendLine($"- Room {r.RoomNumber}: {r.RoomType}, Capacity: {r.Capacity}, Price per night: {r.PricePerNight}, Status: {r.Status}");
            }

            context.AppendLine("\nBookings:");
            foreach (var b in bookings.Where(b => b != null).Take(10)) // limit tránh prompt quá dài
            {
                context.AppendLine($"- Booking {b!.BookingID}: User {b.UserID}, Room {b.RoomID}, From {b.CheckInDate} To {b.CheckOutDate}, Status: {b.Status}");
            }

            // 3. Prompt = system + context + user message
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are an AI assistant for NTT hotel booking system.");
            promptBuilder.AppendLine("Always answer based only on the given database information.");
            promptBuilder.AppendLine("If the user asks something outside the data, politely say you don't know.");

            var missingInformationInstruction = BuildRoomSearchGuidance(message, hotels.Select(h => h.City));
            if (!string.IsNullOrEmpty(missingInformationInstruction))
            {
                promptBuilder.AppendLine(missingInformationInstruction);
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Database context: {context}");
            promptBuilder.AppendLine($"User question: {message}");

            var prompt = promptBuilder.ToString();

            // 4. Gọi AI
            var result = await _generativeModel.GenerateContent(prompt);
            var response = result.Text ?? string.Empty;

            // 5. Save chat log
            var chat = new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = userId,
                Message = message,
                Response = response,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(chat);
            await _repository.SaveAsync();

            return chat;
        }

        private static string BuildRoomSearchGuidance(string userMessage, IEnumerable<string> cities)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return string.Empty;

            var normalizedMessage = userMessage.ToLowerInvariant();
            var accentlessMessage = RemoveDiacritics(normalizedMessage);

            var searchKeywords = new[]
            {
                "tìm phòng", "tim phong", "book room", "find room", "đặt phòng", "dat phong"
            };

            if (!searchKeywords.Any(keyword => accentlessMessage.Contains(keyword)))
                return string.Empty;

            var missingDetails = new List<string>();

            if (!HasCityMention(normalizedMessage, accentlessMessage, cities))
            {
                missingDetails.Add("thành phố hoặc địa điểm");
            }

            if (!HasBudgetMention(normalizedMessage, accentlessMessage))
            {
                missingDetails.Add("khoảng ngân sách");
            }

            if (!HasGuestCountMention(normalizedMessage, accentlessMessage))
            {
                missingDetails.Add("số lượng khách");
            }

            if (!missingDetails.Any())
            {
                return "Khi người dùng đã cung cấp đủ thành phố, ngân sách và số lượng khách, hãy đưa ra gợi ý phòng phù hợp dựa trên dữ liệu hiện có.";
            }

            var detailsSentence = string.Join(", ", missingDetails);
            return $"Người dùng đang muốn tìm phòng nhưng chưa cung cấp {detailsSentence}. Hãy hỏi thêm những thông tin này (theo từng câu hỏi riêng) trước khi đưa ra gợi ý.";
        }

        private static bool HasCityMention(string normalizedMessage, string accentlessMessage, IEnumerable<string> cities)
        {
            foreach (var city in cities)
            {
                if (string.IsNullOrWhiteSpace(city))
                    continue;

                var normalizedCity = city.ToLowerInvariant();
                var accentlessCity = RemoveDiacritics(normalizedCity);

                if (normalizedMessage.Contains(normalizedCity) || accentlessMessage.Contains(accentlessCity))
                    return true;
            }

            return false;
        }

        private static bool HasBudgetMention(string normalizedMessage, string accentlessMessage)
        {
            if (Regex.IsMatch(normalizedMessage, "\\b(ngân\s*sách|giá)\\b", RegexOptions.CultureInvariant))
                return true;

            if (Regex.IsMatch(accentlessMessage, "\\b(ngan\s*sach|gia|budget|price)\\b", RegexOptions.CultureInvariant))
                return true;

            return Regex.IsMatch(accentlessMessage, "\\d{3,}");
        }

        private static bool HasGuestCountMention(string normalizedMessage, string accentlessMessage)
        {
            if (Regex.IsMatch(normalizedMessage, "\\b(\\d+)\\s*(người|khách)\\b", RegexOptions.CultureInvariant))
                return true;

            if (Regex.IsMatch(accentlessMessage, "\\b(\\d+)\\s*(nguoi|khach|people|guest)s?\\b", RegexOptions.CultureInvariant))
                return true;

            return Regex.IsMatch(accentlessMessage, "\\b(single|double|twin|family)\\b", RegexOptions.CultureInvariant);
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
