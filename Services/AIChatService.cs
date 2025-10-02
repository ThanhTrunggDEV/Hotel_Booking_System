using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly ConcurrentDictionary<string, RoomSearchState> _roomSearchStates = new();

        public AIChatService(
            IAIChatRepository repository,
            GeminiOptions options,
            IBookingRepository bookingRepository,
            IHotelRepository hotelRepository,
            IRoomRepository roomRepository,
            IReviewRepository reviewRepository)
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

            var hotels = await _hotelRepository.GetAllAsync();
            var rooms = await _roomRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetBookingByUserId(userId);
            var reviews = await _reviewRepository.GetAllAsync();

            var ratingsByHotel = reviews
                .GroupBy(r => r.HotelID)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            if (TryHandleGuidedRoomSearch(userId, message, hotels, rooms, ratingsByHotel, out var guidedChat))
            {
                await _repository.AddAsync(guidedChat);
                await _repository.SaveAsync();
                return guidedChat;
            }

            var context = new StringBuilder();
            context.AppendLine("Dưới đây là ảnh chụp dữ liệu hiện có:");
            context.AppendLine("\nDanh sách khách sạn:");
            foreach (var h in hotels)
            {
                ratingsByHotel.TryGetValue(h.HotelID, out var userRating);
                context.AppendLine($"- {h.HotelID}, {h.HotelName}, Địa chỉ: {h.Address}, {h.City}, Khoảng giá: {h.MinPrice}-{h.MaxPrice}, Đánh giá hệ thống: {h.Rating}, Điểm khách hàng: {userRating:F1}");
            }

            context.AppendLine("\nDanh sách phòng:");
            foreach (var r in rooms)
            {
                context.AppendLine($"- Phòng {r.RoomNumber}: {r.RoomType}, Sức chứa: {r.Capacity}, Giá/đêm: {r.PricePerNight}, Trạng thái: {r.Status}");
            }

            context.AppendLine("\nMột số đặt phòng gần đây của người dùng:");
            foreach (var b in bookings.Where(b => b != null).Take(10))
            {
                context.AppendLine($"- Mã {b!.BookingID}: Người dùng {b.UserID}, Phòng {b.RoomID}, Từ {b.CheckInDate} đến {b.CheckOutDate}, Trạng thái: {b.Status}");
            }

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Bạn là trợ lý du lịch ảo của hệ thống đặt phòng khách sạn NTT.");
            promptBuilder.AppendLine("Yêu cầu bắt buộc:");
            promptBuilder.AppendLine("1. Luôn trả lời 100% bằng tiếng Việt tự nhiên, ngắn gọn, súc tích.");
            promptBuilder.AppendLine("2. Chỉ sử dụng dữ liệu được cung cấp trong ngữ cảnh. Nếu không đủ thông tin, hãy nói rõ rằng bạn không có dữ liệu.");
            promptBuilder.AppendLine("3. Không được suy đoán hoặc sáng tạo thông tin mới, không được viện dẫn nguồn bên ngoài.");
            promptBuilder.AppendLine("4. Khi cần thêm thông tin để hỗ trợ việc tìm phòng, hãy hỏi từng câu riêng biệt và chờ phản hồi.");
            promptBuilder.AppendLine("5. Nếu người dùng hủy hoặc không muốn tiếp tục, hãy trả lời lịch sự và không cung cấp thêm gợi ý.");

            var missingInformationInstruction = BuildRoomSearchGuidance(message, hotels.Select(h => h.City));
            if (!string.IsNullOrEmpty(missingInformationInstruction))
            {
                promptBuilder.AppendLine();
                promptBuilder.AppendLine(missingInformationInstruction);
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Ngữ cảnh dữ liệu (chỉ dùng để tham khảo khi cần trả lời):");
            promptBuilder.AppendLine(context.ToString());
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Câu hỏi của người dùng:");
            promptBuilder.AppendLine(message);

            var prompt = promptBuilder.ToString();

            var result = await _generativeModel.GenerateContent(prompt);
            var response = result.Text ?? string.Empty;

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

        private bool TryHandleGuidedRoomSearch(
            string userId,
            string message,
            IEnumerable<Hotel> hotels,
            IEnumerable<Room> rooms,
            IReadOnlyDictionary<string, double> ratingsByHotel,
            out AIChat chat)
        {
            chat = null!;
            var normalizedMessage = message.ToLowerInvariant();
            var accentlessMessage = RemoveDiacritics(normalizedMessage);

            var state = _roomSearchStates.GetOrAdd(userId, _ => new RoomSearchState());
            var wasActive = state.IsActive;

            if (IsResetRequest(accentlessMessage))
            {
                state.Reset();
                if (wasActive)
                {
                    chat = new AIChat
                    {
                        ChatID = Guid.NewGuid().ToString(),
                        UserID = userId,
                        Message = message,
                        Response = "Không sao, khi nào cần tìm phòng nữa cứ nói với tôi nhé!",
                        CreatedAt = DateTime.UtcNow
                    };
                    return true;
                }

                return false;
            }

            if (!state.IsActive && !IsRoomSearchRequest(accentlessMessage))
            {
                return false;
            }

            state.IsActive = true;
            UpdateCity(state, normalizedMessage, accentlessMessage, hotels.Select(h => h.City));
            UpdateBudget(state, message);
            UpdateGuests(state, accentlessMessage);

            var nextQuestion = GetNextMissingField(state);
            string response;

            if (nextQuestion != null)
            {
                response = nextQuestion switch
                {
                    MissingField.City => "Bạn muốn tìm khách sạn ở thành phố hoặc khu vực nào?",
                    MissingField.Budget => "Ngân sách mỗi đêm của bạn khoảng bao nhiêu?",
                    MissingField.GuestCount => "Bạn dự định có bao nhiêu khách ở cùng?",
                    _ => "Tôi cần thêm vài thông tin nữa để gợi ý chính xác."
                };
            }
            else
            {
                response = BuildRoomSuggestions(state, hotels, rooms, ratingsByHotel);
                state.Reset();
            }

            chat = new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = userId,
                Message = message,
                Response = response,
                CreatedAt = DateTime.UtcNow
            };

            return true;
        }

        private static string BuildRoomSuggestions(
            RoomSearchState state,
            IEnumerable<Hotel> hotels,
            IEnumerable<Room> rooms,
            IReadOnlyDictionary<string, double> ratingsByHotel)
        {
            var comparisonCity = RemoveDiacritics(state.City ?? string.Empty).ToLowerInvariant();

            var matchingHotels = hotels
                .Where(h => !string.IsNullOrWhiteSpace(h.City))
                .Where(h => RemoveDiacritics(h.City).ToLowerInvariant().Contains(comparisonCity) ||
                            RemoveDiacritics(h.Address).ToLowerInvariant().Contains(comparisonCity))
                .ToList();

            if (!matchingHotels.Any())
            {
                return "Hiện tại tôi chưa tìm thấy khách sạn nào phù hợp với địa điểm bạn yêu cầu. Bạn có thể thử với khu vực khác không?";
            }

            double? minBudget = state.MinBudget;
            double? maxBudget = state.MaxBudget;
            if (minBudget.HasValue && maxBudget.HasValue && minBudget > maxBudget)
            {
                (minBudget, maxBudget) = (maxBudget, minBudget);
            }

            var guestCount = state.GuestCount ?? 1;

            var sb = new StringBuilder();
            sb.AppendLine($"Dưới đây là một vài gợi ý phù hợp tại {state.City}:");

            var suggestionsAdded = 0;
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            foreach (var hotel in matchingHotels)
            {
                var suitableRooms = rooms
                    .Where(r => r.HotelID == hotel.HotelID)
                    .Where(r => r.Capacity >= guestCount)
                    .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
                    .Where(r => (!minBudget.HasValue || r.PricePerNight >= minBudget) &&
                                (!maxBudget.HasValue || r.PricePerNight <= maxBudget))
                    .OrderBy(r => r.PricePerNight)
                    .Take(2)
                    .ToList();

                if (!suitableRooms.Any())
                {
                    continue;
                }

                suggestionsAdded++;
                ratingsByHotel.TryGetValue(hotel.HotelID, out var userRating);
                sb.AppendLine($"{suggestionsAdded}. {hotel.HotelName} - {hotel.Address}. Hạng khách sạn: {hotel.Rating:F1}/5, điểm đánh giá khách: {userRating:F1}/5.");

                foreach (var room in suitableRooms)
                {
                    sb.AppendLine($"   • Phòng {room.RoomNumber} ({room.RoomType}), sức chứa {room.Capacity} người, giá {room.PricePerNight.ToString("C0", culture)}/đêm.");
                }

                if (suggestionsAdded >= 3)
                {
                    break;
                }
            }

            if (suggestionsAdded == 0)
            {
                return "Tôi chưa tìm thấy phòng nào phù hợp với ngân sách và số lượng khách bạn đưa ra. Bạn có muốn điều chỉnh lại thông tin không?";
            }

            sb.AppendLine("Nếu bạn cần thêm lựa chọn hoặc muốn thay đổi thông tin, cứ cho tôi biết nhé!");
            return sb.ToString().TrimEnd();
        }

        private static void UpdateCity(RoomSearchState state, string normalizedMessage, string accentlessMessage, IEnumerable<string> cities)
        {
            foreach (var city in cities)
            {
                if (string.IsNullOrWhiteSpace(city))
                    continue;

                var normalizedCity = city.ToLowerInvariant();
                var accentlessCity = RemoveDiacritics(normalizedCity);

                if (normalizedMessage.Contains(normalizedCity) || accentlessMessage.Contains(accentlessCity))
                {
                    state.City = city;
                    return;
                }
            }
        }

        private static void UpdateBudget(RoomSearchState state, string originalMessage)
        {
            var (min, max) = ExtractBudgetRange(originalMessage);
            if (min.HasValue)
            {
                state.MinBudget = min.Value;
            }
            if (max.HasValue)
            {
                state.MaxBudget = max.Value;
            }
        }

        private static void UpdateGuests(RoomSearchState state, string accentlessMessage)
        {
            var guests = ExtractGuestCount(accentlessMessage);
            if (guests.HasValue)
            {
                state.GuestCount = guests.Value;
            }
        }

        private static bool IsResetRequest(string accentlessMessage)
        {
            return accentlessMessage.Contains("huy") || accentlessMessage.Contains("khong can");
        }

        private static bool IsRoomSearchRequest(string accentlessMessage)
        {
            var searchKeywords = new[]
            {
                "tim phong", "tim khach san", "dat phong", "dat khach san", "find room", "find hotel", "book room", "book hotel"
            };

            return searchKeywords.Any(keyword => accentlessMessage.Contains(keyword));
        }

        private static MissingField? GetNextMissingField(RoomSearchState state)
        {
            if (string.IsNullOrWhiteSpace(state.City))
                return MissingField.City;
            if (!state.MinBudget.HasValue && !state.MaxBudget.HasValue)
                return MissingField.Budget;
            if (!state.GuestCount.HasValue)
                return MissingField.GuestCount;
            return null;
        }

        private static (double? min, double? max) ExtractBudgetRange(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return (null, null);

            var matches = Regex.Matches(message, @"(?<value>\d+(?:[\.,]\d+)?)\s*(?<unit>trieu|tr|triệu|tr\.|k|ngan|ngàn|nghin|nghìn|vnđ|vnd|đ|d)?", RegexOptions.IgnoreCase);
            if (matches.Count == 0)
                return (null, null);

            var values = new List<double>();
            foreach (Match match in matches)
            {
                var numericPart = match.Groups["value"].Value.Replace(" ", string.Empty);
                var parsed = false;
                if (double.TryParse(numericPart.Replace(".", string.Empty).Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
                {
                    values.Add(ApplyUnit(invariantValue, match.Groups["unit"].Value));
                    parsed = true;
                }

                if (!parsed && double.TryParse(numericPart, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out var viValue))
                {
                    values.Add(ApplyUnit(viValue, match.Groups["unit"].Value));
                }
            }

            if (!values.Any())
                return (null, null);

            var min = values.Min();
            var max = values.Max();
            return (min, values.Count > 1 ? max : min);
        }

        private static double ApplyUnit(double value, string unitRaw)
        {
            var unit = unitRaw.ToLowerInvariant();
            return unit switch
            {
                "trieu" => value * 1_000_000,
                "triệu" => value * 1_000_000,
                "tr" => value * 1_000_000,
                "tr." => value * 1_000_000,
                "k" => value * 1_000,
                "ngan" => value * 1_000,
                "ngàn" => value * 1_000,
                "nghin" => value * 1_000,
                "nghìn" => value * 1_000,
                _ => value
            };
        }

        private static int? ExtractGuestCount(string accentlessMessage)
        {
            var match = Regex.Match(accentlessMessage, @"(\d+)\s*(nguoi|khach|people|guest)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var guests))
            {
                return guests;
            }

            var roomTypeKeywords = new Dictionary<string, int>
            {
                {"single", 1},
                {"double", 2},
                {"twin", 2},
                {"family", 4}
            };

            foreach (var keyword in roomTypeKeywords)
            {
                if (accentlessMessage.Contains(keyword.Key))
                {
                    return keyword.Value;
                }
            }

            return null;
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
                return "Nếu người dùng đã cung cấp đủ thành phố, ngân sách và số lượng khách thì hãy tổng hợp ngắn gọn và đưa ra gợi ý phòng dựa hoàn toàn trên dữ liệu đã cho.";
            }

            var orderedPrompts = missingDetails
                .Select((detail, index) => $"{index + 1}. Hỏi rõ về {detail} (chỉ một câu hỏi).")
                .ToList();

            var guidanceBuilder = new StringBuilder();
            guidanceBuilder.AppendLine("Người dùng đang muốn tìm phòng nhưng thông tin còn thiếu.");
            guidanceBuilder.AppendLine("Thực hiện lần lượt các bước sau trước khi đưa ra gợi ý:");
            foreach (var prompt in orderedPrompts)
            {
                guidanceBuilder.AppendLine($"- {prompt}");
            }

            guidanceBuilder.Append("Luôn chờ câu trả lời của người dùng trước khi chuyển sang câu hỏi tiếp theo.");
            return guidanceBuilder.ToString();
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
            if (Regex.IsMatch(normalizedMessage, @"\b(ngân\s*sách|giá)\b", RegexOptions.CultureInvariant))
                return true;

            if (Regex.IsMatch(accentlessMessage, @"\b(ngan\s*sach|gia|budget|price)\b", RegexOptions.CultureInvariant))
                return true;

            return Regex.IsMatch(accentlessMessage, @"\d{3,}");
        }

        private static bool HasGuestCountMention(string normalizedMessage, string accentlessMessage)
        {
            if (Regex.IsMatch(normalizedMessage, @"\b(\d+)\s*(người|khách)\b", RegexOptions.CultureInvariant))
                return true;

            if (Regex.IsMatch(accentlessMessage, @"\b(\d+)\s*(nguoi|khach|people|guest)s?\b", RegexOptions.CultureInvariant))
                return true;

            return Regex.IsMatch(accentlessMessage, @"\b(single|double|twin|family)\b", RegexOptions.CultureInvariant);
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

        private enum MissingField
        {
            City,
            Budget,
            GuestCount
        }

        private sealed class RoomSearchState
        {
            public bool IsActive { get; set; }
            public string? City { get; set; }
            public double? MinBudget { get; set; }
            public double? MaxBudget { get; set; }
            public int? GuestCount { get; set; }

            public void Reset()
            {
                IsActive = false;
                City = null;
                MinBudget = null;
                MaxBudget = null;
                GuestCount = null;
            }
        }
    }
}
