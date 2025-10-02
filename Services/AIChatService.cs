using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;
        private readonly GeminiOptions _options;
        private readonly HttpClient _httpClient;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<AIChatService> _logger;
        private readonly ConcurrentDictionary<string, RoomSearchState> _roomSearchStates = new();

        public AIChatService(
            IAIChatRepository repository,
            GeminiOptions options,
            HttpClient httpClient,
            IBookingRepository bookingRepository,
            IHotelRepository hotelRepository,
            IRoomRepository roomRepository,
            IReviewRepository reviewRepository,
            ILogger<AIChatService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var history = await _repository.GetByUserId(userId);
            var orderedHistory = history
                .OrderBy(c => c.CreatedAt)
                .TakeLast(10)
                .ToList();

            var resolvedModel = string.IsNullOrWhiteSpace(model) ? _options.DefaultModel : model;
            var systemPrompt = BuildSystemPrompt(message, hotels, rooms, ratingsByHotel, bookings);
            var payload = BuildPayload(systemPrompt, orderedHistory, message);

            var responseText = await GenerateModelResponseAsync(payload, resolvedModel);
            var normalizedResponse = NormalizeResponse(responseText);

            var chat = new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = userId,
                Message = message,
                Response = normalizedResponse,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(chat);
            await _repository.SaveAsync();

            return chat;
        }

        private async Task<string> GenerateModelResponseAsync(object payload, string? modelName)
        {
            var apiKey = ResolveApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Xin lỗi, trợ lý AI chưa được cấu hình khóa truy cập. Bạn vui lòng liên hệ quản trị viên giúp mình nhé.";
            }

            var endpoint = BuildEndpoint(apiKey, modelName);

            const int maxAttempts = 3;
            var delay = TimeSpan.FromSeconds(1);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        if (ShouldRetry(response.StatusCode) && attempt < maxAttempts)
                        {
                            _logger.LogWarning("Gemini API trả về lỗi tạm thời {StatusCode} (attempt {Attempt}/{MaxAttempts}). Sẽ thử lại sau {Delay} giây.",
                                (int)response.StatusCode, attempt, maxAttempts, delay.TotalSeconds);
                        }
                        else
                        {
                            _logger.LogError("Gemini API trả về lỗi {StatusCode}: {Body}", (int)response.StatusCode, responseBody);
                            return "Xin lỗi, hiện mình chưa kết nối được với trợ lý AI. Bạn vui lòng thử lại sau ít phút nhé.";
                        }
                    }
                    else
                    {
                        using var document = JsonDocument.Parse(responseBody);
                        var text = ExtractText(document);

                        if (string.IsNullOrWhiteSpace(text))
                        {
                            _logger.LogWarning("Gemini API không trả về nội dung văn bản.");
                            return "Mình chưa nhận được phản hồi phù hợp. Bạn có thể diễn đạt lại yêu cầu giúp mình không?";
                        }

                        return StripCodeFence(text.Trim());
                    }
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt < maxAttempts)
                {
                    _logger.LogWarning(ex, "Không thể gọi Gemini API ở lần thử {Attempt}/{MaxAttempts}. Sẽ thử lại sau {Delay} giây.",
                        attempt, maxAttempts, delay.TotalSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể gọi Gemini API");
                    return "Xin lỗi, trợ lý AI đang bận. Bạn vui lòng thử lại sau một lúc nhé.";
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 8));
                }
            }

            _logger.LogError("Gemini API vẫn không phản hồi sau {MaxAttempts} lần thử.", maxAttempts);
            return "Xin lỗi, hiện mình chưa thể truy vấn được dữ liệu từ trợ lý AI. Bạn hãy hỏi lại sau ít phút nhé.";
        }

        private object BuildPayload(string systemPrompt, IReadOnlyCollection<AIChat> history, string latestMessage)
        {
            var contents = new List<object>();

            foreach (var entry in history)
            {
                if (!string.IsNullOrWhiteSpace(entry.Message))
                {
                    contents.Add(new
                    {
                        role = "user",
                        parts = new[] { new { text = entry.Message } }
                    });
                }

                if (!string.IsNullOrWhiteSpace(entry.Response))
                {
                    contents.Add(new
                    {
                        role = "model",
                        parts = new[] { new { text = entry.Response } }
                    });
                }
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = latestMessage } }
            });

            return new
            {
                systemInstruction = new
                {
                    role = "system",
                    parts = new[]
                    {
                        new { text = systemPrompt }
                    }
                },
                contents,
                generationConfig = new
                {
                    temperature = 0.4,
                    topP = 0.9,
                    topK = 40,
                    maxOutputTokens = _options.MaxOutputTokens
                }
            };
        }

        private string BuildSystemPrompt(
            string userMessage,
            IEnumerable<Hotel> hotels,
            IEnumerable<Room> rooms,
            IReadOnlyDictionary<string, double> ratingsByHotel,
            IEnumerable<Booking?> bookings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Bạn là trợ lý đặt phòng khách sạn của hệ thống NTT.");
            builder.AppendLine("Mục tiêu: hỗ trợ người dùng tìm khách sạn và phòng phù hợp dựa trên dữ liệu cung cấp.");
            builder.AppendLine();
            builder.AppendLine("Quy tắc bắt buộc:");
            builder.AppendLine("1. Luôn trả lời 100% bằng tiếng Việt tự nhiên, thân thiện.");
            builder.AppendLine("2. Chỉ sử dụng dữ liệu trong danh sách đã cung cấp. Không tự suy diễn hoặc thêm thông tin mới.");
            builder.AppendLine("3. Nếu thiếu thông tin quan trọng (địa điểm, ngân sách, số khách, thời gian), hãy đặt một câu hỏi ngắn để làm rõ trước khi gợi ý.");
            builder.AppendLine("4. Khi có đủ dữ liệu, hãy đề xuất tối đa 3 khách sạn/phòng còn trống, nêu rõ tên khách sạn, địa chỉ/khu vực, giá tham khảo và điểm nổi bật.");
            builder.AppendLine("5. Nếu không có lựa chọn phù hợp, hãy nói rõ lý do và gợi ý người dùng điều chỉnh yêu cầu.");
            builder.AppendLine("6. Nếu người dùng muốn kết thúc, hãy xác nhận lịch sự và không gợi ý thêm.");

            var missingInformationInstruction = BuildRoomSearchGuidance(userMessage, hotels.Select(h => h.City));
            if (!string.IsNullOrWhiteSpace(missingInformationInstruction))
            {
                builder.AppendLine();
                builder.AppendLine(missingInformationInstruction);
            }

            builder.AppendLine();
            builder.AppendLine("Dữ liệu tham khảo (ưu tiên các phòng trạng thái Available):");

            foreach (var hotel in hotels)
            {
                if (hotel == null)
                {
                    continue;
                }

                var ratingText = ratingsByHotel.TryGetValue(hotel.HotelID, out var userRating)
                    ? userRating.ToString("F1", CultureInfo.InvariantCulture)
                    : "Chưa có";

                builder.AppendLine($"- {hotel.HotelName} (ID {hotel.HotelID}) tại {hotel.Address}, {hotel.City}. Khoảng giá: {hotel.MinPrice}-{hotel.MaxPrice} VNĐ. Hạng hệ thống: {hotel.Rating:F1}/5. Điểm khách đánh giá: {ratingText}.");

                var availableRooms = rooms
                    .Where(r => r.HotelID == hotel.HotelID)
                    .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.PricePerNight)
                    .Take(3)
                    .ToList();

                foreach (var room in availableRooms)
                {
                    builder.AppendLine($"   • Phòng {room.RoomNumber} ({room.RoomType}) - sức chứa {room.Capacity} khách, giá {room.PricePerNight} VNĐ/đêm.");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Một số thông tin đặt phòng gần đây của người dùng:");
            foreach (var booking in bookings.Where(b => b != null).Take(5))
            {
                builder.AppendLine($"- Đặt phòng {booking!.BookingID} tại khách sạn {booking.HotelID}, phòng {booking.RoomID}, từ {booking.CheckInDate:d} đến {booking.CheckOutDate:d}, trạng thái {booking.Status}.");
            }

            builder.AppendLine();
            builder.AppendLine("Tin nhắn mới nhất của người dùng:");
            builder.AppendLine(userMessage);

            return builder.ToString();
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
                var userRatingText = ratingsByHotel.TryGetValue(hotel.HotelID, out var userRating)
                    ? userRating.ToString("F1", CultureInfo.InvariantCulture)
                    : "Chưa có";

                sb.AppendLine($"{suggestionsAdded}. {hotel.HotelName} - {hotel.Address}. Hạng khách sạn: {hotel.Rating:F1}/5, điểm đánh giá khách: {userRatingText}.");

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

        private string NormalizeResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return "Xin lỗi, mình chưa có câu trả lời phù hợp. Bạn có thể thử hỏi lại hoặc cung cấp thêm thông tin nhé.";
            }

            var trimmed = responseText.Trim();
            if (!trimmed.Any(char.IsLetterOrDigit))
            {
                return "Xin lỗi, mình chưa có câu trả lời phù hợp. Bạn thử diễn đạt lại giúp mình nhé.";
            }

            return trimmed;
        }

        private string? ResolveApiKey()
        {
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return _options.ApiKey;
            }

            var environmentApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? Environment.GetEnvironmentVariable("GEMINI__APIKEY")
                ?? Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY");

            if (!string.IsNullOrWhiteSpace(environmentApiKey))
            {
                _options.ApiKey = environmentApiKey.Trim();
                return _options.ApiKey;
            }

            _logger.LogWarning("Gemini API key is not configured. Please set GEMINI_API_KEY or update application settings.");
            return null;
        }

        private string BuildEndpoint(string apiKey, string? modelName)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_options.ApiBaseUrl)
                ? "https://generativelanguage.googleapis.com/v1beta"
                : _options.ApiBaseUrl.TrimEnd('/');

            var model = string.IsNullOrWhiteSpace(modelName)
                ? (string.IsNullOrWhiteSpace(_options.DefaultModel) ? "gemini-2.0-flash" : _options.DefaultModel)
                : modelName;

            return $"{baseUrl}/models/{model}:generateContent?key={apiKey}";
        }

        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.TooManyRequests)
            {
                return true;
            }

            var numericStatus = (int)statusCode;
            return numericStatus >= 500 && numericStatus < 600;
        }

        private static bool IsTransientException(Exception exception)
        {
            return exception switch
            {
                HttpRequestException => true,
                TaskCanceledException => true,
                _ => false
            };
        }

        private static string? ExtractText(JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var content = candidates[0].GetProperty("content");
            if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
            {
                return null;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return null;
        }

        private static string StripCodeFence(string text)
        {
            var trimmed = text.Trim();
            if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                return trimmed;
            }

            var endIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (endIndex <= 0)
            {
                return trimmed;
            }

            var newlineIndex = trimmed.IndexOf('\n');
            if (newlineIndex >= 0 && newlineIndex < endIndex)
            {
                return trimmed[(newlineIndex + 1)..endIndex].Trim();
            }

            var spaceIndex = trimmed.IndexOf(' ');
            if (spaceIndex >= 0 && spaceIndex < endIndex)
            {
                return trimmed[(spaceIndex + 1)..endIndex].Trim();
            }

            return trimmed[3..endIndex].Trim();
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
