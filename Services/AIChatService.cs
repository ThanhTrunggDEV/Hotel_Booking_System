using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            var hotels = (await _hotelRepository.GetAllAsync()).ToList();
            var rooms = (await _roomRepository.GetAllAsync()).ToList();
            var bookings = await _bookingRepository.GetBookingByUserId(userId);
            var reviews = await _reviewRepository.GetAllAsync();

            var ratingsByHotel = reviews
                .GroupBy(r => r.HotelID)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

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

            var suggestions = BuildSuggestedRooms(message, hotels, rooms, ratingsByHotel);
            if (suggestions.Count > 0)
            {
                normalizedResponse = AppendSuggestionsCallToAction(normalizedResponse);
            }

            var chat = new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = userId,
                Message = message,
                Response = normalizedResponse,
                CreatedAt = DateTime.UtcNow
            };

            if (suggestions.Count > 0)
            {
                chat.SuggestedRooms = new ObservableCollection<ChatRoomSuggestion>(suggestions);
            }

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
            builder.AppendLine("3. Chủ động đề xuất tối đa 3 khách sạn/phòng còn trống, nêu rõ tên khách sạn, địa chỉ/khu vực, giá tham khảo, sức chứa và điểm nổi bật.");
            builder.AppendLine("4. Nếu thông tin người dùng chưa đầy đủ, hãy đưa ra gợi ý phù hợp nhất có thể và mời họ điều chỉnh thêm khi cần.");
            builder.AppendLine("5. Hãy nhắc người dùng có thể nhấn nút \"Đặt phòng\" trong khung chat khi thấy lựa chọn phù hợp.");
            builder.AppendLine("6. Nếu không có lựa chọn phù hợp, hãy nói rõ lý do và gợi ý người dùng điều chỉnh yêu cầu.");
            builder.AppendLine("7. Nếu người dùng muốn kết thúc, hãy xác nhận lịch sự và không gợi ý thêm.");

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

        private IReadOnlyList<ChatRoomSuggestion> BuildSuggestedRooms(
            string message,
            IReadOnlyCollection<Hotel> hotels,
            IReadOnlyCollection<Room> rooms,
            IReadOnlyDictionary<string, double> ratingsByHotel)
        {
            if (string.IsNullOrWhiteSpace(message) || hotels.Count == 0 || rooms.Count == 0)
            {
                return Array.Empty<ChatRoomSuggestion>();
            }

            var normalizedMessage = message.ToLowerInvariant();
            var accentlessMessage = RemoveDiacritics(normalizedMessage);

            var orderedHotels = hotels
                .Where(h => h != null)
                .OrderByDescending(h => ratingsByHotel.TryGetValue(h.HotelID, out var userRating) ? userRating : h.Rating)
                .ThenBy(h => h.MinPrice)
                .ToList();

            if (orderedHotels.Count == 0)
            {
                return Array.Empty<ChatRoomSuggestion>();
            }

            var focusCity = FindCityFromMessage(normalizedMessage, accentlessMessage, orderedHotels);
            var filteredHotels = FilterHotelsForMessage(orderedHotels, normalizedMessage, accentlessMessage, focusCity);

            var (minBudget, maxBudget) = ExtractBudgetRange(message);
            var guestCount = ExtractGuestCount(accentlessMessage) ?? 1;

            var suggestions = new List<ChatRoomSuggestion>();

            foreach (var hotel in filteredHotels)
            {
                var availableRooms = rooms
                    .Where(r => r.HotelID == hotel.HotelID)
                    .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.PricePerNight)
                    .ToList();

                if (availableRooms.Count == 0)
                {
                    continue;
                }

                var matchingRooms = availableRooms
                    .Where(r => r.Capacity >= guestCount)
                    .Where(r => (!minBudget.HasValue || r.PricePerNight >= minBudget.Value) &&
                                (!maxBudget.HasValue || r.PricePerNight <= maxBudget.Value))
                    .ToList();

                if (matchingRooms.Count == 0)
                {
                    matchingRooms = availableRooms
                        .Where(r => r.Capacity >= guestCount)
                        .ToList();
                }

                foreach (var room in matchingRooms)
                {
                    suggestions.Add(new ChatRoomSuggestion
                    {
                        Hotel = hotel,
                        Room = room,
                        UserRating = ratingsByHotel.TryGetValue(hotel.HotelID, out var rating) ? rating : null
                    });

                    if (suggestions.Count >= 3)
                    {
                        return suggestions;
                    }
                }
            }

            if (suggestions.Count == 0)
            {
                var fallbackRooms = rooms
                    .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.PricePerNight)
                    .Take(3)
                    .ToList();

                foreach (var room in fallbackRooms)
                {
                    var hotel = orderedHotels.FirstOrDefault(h => h.HotelID == room.HotelID);
                    if (hotel == null)
                    {
                        continue;
                    }

                    suggestions.Add(new ChatRoomSuggestion
                    {
                        Hotel = hotel,
                        Room = room,
                        UserRating = ratingsByHotel.TryGetValue(hotel.HotelID, out var rating) ? rating : null
                    });
                }
            }

            return suggestions;
        }

        private static IReadOnlyList<Hotel> FilterHotelsForMessage(
            IReadOnlyList<Hotel> orderedHotels,
            string normalizedMessage,
            string accentlessMessage,
            string? focusCity)
        {
            if (string.IsNullOrWhiteSpace(focusCity))
            {
                return orderedHotels.ToList();
            }

            var accentlessFocus = RemoveDiacritics(focusCity.ToLowerInvariant());

            var filtered = orderedHotels
                .Where(h => LocationMatchesMessage(h, normalizedMessage, accentlessMessage, accentlessFocus))
                .ToList();

            return filtered.Count > 0 ? filtered : orderedHotels.ToList();
        }

        private static bool LocationMatchesMessage(
            Hotel hotel,
            string normalizedMessage,
            string accentlessMessage,
            string accentlessFocus)
        {
            var normalizedCity = (hotel.City ?? string.Empty).ToLowerInvariant();
            var accentlessCity = RemoveDiacritics(normalizedCity);
            var normalizedAddress = (hotel.Address ?? string.Empty).ToLowerInvariant();
            var accentlessAddress = RemoveDiacritics(normalizedAddress);

            if (!string.IsNullOrWhiteSpace(accentlessFocus))
            {
                if (!string.IsNullOrWhiteSpace(accentlessCity) &&
                    (accentlessCity.Contains(accentlessFocus, StringComparison.OrdinalIgnoreCase) ||
                     accentlessFocus.Contains(accentlessCity, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(accentlessAddress) &&
                    accentlessAddress.Contains(accentlessFocus, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return (!string.IsNullOrWhiteSpace(normalizedCity) &&
                    (normalizedMessage.Contains(normalizedCity, StringComparison.OrdinalIgnoreCase) ||
                     accentlessMessage.Contains(accentlessCity, StringComparison.OrdinalIgnoreCase))) ||
                   (!string.IsNullOrWhiteSpace(normalizedAddress) &&
                    (normalizedMessage.Contains(normalizedAddress, StringComparison.OrdinalIgnoreCase) ||
                     accentlessMessage.Contains(accentlessAddress, StringComparison.OrdinalIgnoreCase)));
        }

        private static string? FindCityFromMessage(
            string normalizedMessage,
            string accentlessMessage,
            IEnumerable<Hotel> hotels)
        {
            foreach (var hotel in hotels)
            {
                if (hotel == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(hotel.City))
                {
                    var normalizedCity = hotel.City.ToLowerInvariant();
                    var accentlessCity = RemoveDiacritics(normalizedCity);

                    if (normalizedMessage.Contains(normalizedCity, StringComparison.OrdinalIgnoreCase) ||
                        accentlessMessage.Contains(accentlessCity, StringComparison.OrdinalIgnoreCase))
                    {
                        return hotel.City;
                    }
                }

                if (!string.IsNullOrWhiteSpace(hotel.Address))
                {
                    var normalizedAddress = hotel.Address.ToLowerInvariant();
                    var accentlessAddress = RemoveDiacritics(normalizedAddress);

                    if (normalizedMessage.Contains(normalizedAddress, StringComparison.OrdinalIgnoreCase) ||
                        accentlessMessage.Contains(accentlessAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        return hotel.City;
                    }
                }
            }

            return null;
        }

        private static string AppendSuggestionsCallToAction(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return "Mình đã chuẩn bị vài gợi ý ở bên dưới, bạn có thể bấm \"Đặt phòng\" để đặt ngay nhé!";
            }

            if (response.Contains("đặt phòng", StringComparison.OrdinalIgnoreCase))
            {
                return response;
            }

            return $"{response.TrimEnd()}\n\nMình đã đính kèm một vài lựa chọn phù hợp phía dưới, bạn có thể bấm \"Đặt phòng\" để giữ chỗ ngay nhé!";
        }

        private static string NormalizeResponse(string? response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return "Mình chưa nhận được phản hồi phù hợp. Bạn có thể hỏi lại giúp mình nhé!";
            }

            var normalized = response
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n')
                .Trim();

            normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
            normalized = Regex.Replace(normalized, "[ \t]{2,}", " ");

            return normalized;
        }

        private string ResolveApiKey()
        {
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return _options.ApiKey;
            }

            var fromEnvironment = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
            {
                return fromEnvironment;
            }

            _logger.LogWarning("Gemini API key is not configured in options or environment.");
            return string.Empty;
        }

        private string BuildEndpoint(string apiKey, string? modelName)
        {
            var baseUrl = string.IsNullOrWhiteSpace(_options.ApiBaseUrl)
                ? "https://generativelanguage.googleapis.com/v1beta"
                : _options.ApiBaseUrl.TrimEnd('/');

            var effectiveModel = string.IsNullOrWhiteSpace(modelName) ? _options.DefaultModel : modelName!;
            var encodedModel = Uri.EscapeDataString(effectiveModel);

            return $"{baseUrl}/models/{encodedModel}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        }

        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout ||
                   statusCode == (HttpStatusCode)429 ||
                   (int)statusCode >= 500;
        }

        private static string ExtractText(JsonDocument document)
        {
            if (document.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array)
            {
                foreach (var candidate in candidates.EnumerateArray())
                {
                    if (candidate.TryGetProperty("content", out var content))
                    {
                        if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var part in parts.EnumerateArray())
                            {
                                if (part.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                                {
                                    return textElement.GetString() ?? string.Empty;
                                }
                            }
                        }
                    }

                    if (candidate.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in output.EnumerateArray())
                        {
                            if (item.TryGetProperty("content", out var innerContent) &&
                                innerContent.TryGetProperty("parts", out var innerParts) &&
                                innerParts.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var innerPart in innerParts.EnumerateArray())
                                {
                                    if (innerPart.TryGetProperty("text", out var innerText) && innerText.ValueKind == JsonValueKind.String)
                                    {
                                        return innerText.GetString() ?? string.Empty;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (document.RootElement.TryGetProperty("text", out var rootText) && rootText.ValueKind == JsonValueKind.String)
            {
                return rootText.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static bool IsTransientException(Exception ex)
        {
            return ex is HttpRequestException ||
                   ex is TaskCanceledException ||
                   ex is TimeoutException ||
                   ex is OperationCanceledException;
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
