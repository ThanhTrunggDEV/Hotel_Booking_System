using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private const string DefaultApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private readonly IAIChatRepository _repository;
        private readonly GeminiOptions _options;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(
            IAIChatRepository repository,
            GeminiOptions options,
            IBookingRepository bookingRepository,
            IHotelRepository hotelRepository,
            IRoomRepository roomRepository,
            IReviewRepository reviewRepository,
            HttpClient httpClient,
            ILogger<AIChatService> logger)
        {
            _repository = repository;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
            _options = options;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AIChat> SendAsync(
            string userId,
            string message,
            string? model = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            var apiKey = ResolveApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return CreateFallbackChat(userId, message,
                    "Tính năng trợ lý AI đang được cấu hình. Vui lòng liên hệ quản trị viên để cung cấp khóa API Gemini.");
            }

            var hotelsTask = _hotelRepository.GetAllAsync();
            var roomsTask = _roomRepository.GetAllAsync();
            var bookingsTask = _bookingRepository.GetBookingByUserId(userId);
            var reviewsTask = _reviewRepository.GetAllAsync();
            var historyTask = _repository.GetByUserId(userId);

            await Task.WhenAll(hotelsTask, roomsTask, bookingsTask, reviewsTask, historyTask);

            cancellationToken.ThrowIfCancellationRequested();

            var hotels = hotelsTask.Result;
            var rooms = roomsTask.Result;
            var bookings = bookingsTask.Result;
            var reviews = reviewsTask.Result;
            var history = historyTask.Result
                .OrderBy(c => c.CreatedAt)
                .TakeLast(12)
                .ToList();

            var ratingsByHotel = reviews
                .GroupBy(r => r.HotelID)
                .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

            var hotelLookup = hotels.ToDictionary(h => h.HotelID);
            var roomLookup = rooms.ToDictionary(r => r.RoomID);

            var systemPrompt = BuildSystemPrompt(message, hotels, rooms, bookings, ratingsByHotel);
            var payload = BuildPayload(systemPrompt, history, message);

            var modelName = !string.IsNullOrWhiteSpace(model)
                ? model
                : Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? _options.DefaultModel;

            var endpoint = BuildEndpoint(apiKey, modelName);

            const int maxAttempts = 3;
            var delay = TimeSpan.FromSeconds(1);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using var response = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
                    var serverSuggestedDelay = GetServerSuggestedDelay(response);
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        var statusCode = response.StatusCode;

                        if (ShouldRetry(statusCode) && attempt < maxAttempts)
                        {
                            var nextDelay = CalculateNextDelay(delay, serverSuggestedDelay);
                            _logger.LogWarning(
                                "Gemini API returned status {StatusCode} on attempt {Attempt}/{MaxAttempts}. Retrying after {Delay}s.",
                                (int)statusCode, attempt, maxAttempts, nextDelay.TotalSeconds);
                            await Task.Delay(nextDelay, cancellationToken);
                            delay = nextDelay;
                            continue;
                        }
                        else
                        {
                            _logger.LogError("Gemini API error {StatusCode}: {Body}", (int)statusCode, responseBody);
                            return CreateFallbackChat(userId, message,
                                "Xin lỗi, trợ lý AI đang gặp sự cố khi kết nối. Bạn vui lòng thử lại sau nhé.");
                        }
                    }
                    else
                    {
                        using var document = JsonDocument.Parse(responseBody);
                        var text = ExtractText(document);

                        if (string.IsNullOrWhiteSpace(text))
                        {
                            _logger.LogWarning("Gemini API không trả về nội dung văn bản.");
                            return CreateFallbackChat(userId, message,
                                "Mình chưa nhận được câu trả lời từ AI. Bạn có thể thử hỏi lại giúp mình không?");
                        }

                        var modelResponse = ParseModelResponse(text);
                        var displayResponse = BuildDisplayResponse(modelResponse, hotelLookup, roomLookup);

                        var chat = new AIChat
                        {
                            ChatID = Guid.NewGuid().ToString(),
                            UserID = userId,
                            Message = message,
                            Response = displayResponse,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _repository.AddAsync(chat);
                        await _repository.SaveAsync();

                        return chat;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt < maxAttempts)
                {
                    var nextDelay = CalculateNextDelay(delay, null);
                    _logger.LogWarning(ex,
                        "Không thể gọi Gemini API ở lần thử {Attempt}/{MaxAttempts}. Sẽ thử lại sau {Delay} giây.",
                        attempt, maxAttempts, nextDelay.TotalSeconds);
                    await Task.Delay(nextDelay, cancellationToken);
                    delay = nextDelay;
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể gọi Gemini API");
                    return CreateFallbackChat(userId, message,
                        "Xin lỗi, trợ lý AI không hoạt động tạm thời. Bạn vui lòng thử lại sau nhé.");
                }
            }

            _logger.LogError("Gemini API vẫn không phản hồi sau {MaxAttempts} lần thử.", maxAttempts);
            return CreateFallbackChat(userId, message,
                "Xin lỗi, trợ lý AI đang bận. Bạn vui lòng thử lại sau ít phút nhé.");
        }

        private string BuildSystemPrompt(
            string userMessage,
            IReadOnlyCollection<Hotel> hotels,
            IReadOnlyCollection<Room> rooms,
            IReadOnlyCollection<Booking> bookings,
            IReadOnlyDictionary<string, double> ratingsByHotel)
        {
            var availableHotels = hotels
                .Where(h => h.IsApproved && h.IsVisible)
                .Select(h => new
                {
                    hotelId = h.HotelID,
                    name = h.HotelName,
                    city = h.City,
                    address = h.Address,
                    minPrice = h.MinPrice,
                    maxPrice = h.MaxPrice,
                    starRating = h.Rating,
                    userRating = ratingsByHotel.TryGetValue(h.HotelID, out var rating) ? Math.Round(rating, 1) : (double?)null
                })
                .ToList();

            var availableRooms = rooms
                .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
                .Select(r => new
                {
                    roomId = r.RoomID,
                    hotelId = r.HotelID,
                    roomNumber = r.RoomNumber,
                    roomType = r.RoomType,
                    capacity = r.Capacity,
                    pricePerNight = r.PricePerNight,
                    status = r.Status
                })
                .ToList();

            var upcomingBookings = bookings
                .Where(b => b != null && b.CheckOutDate >= DateTime.UtcNow.Date)
                .OrderByDescending(b => b.CheckInDate)
                .Take(20)
                .Select(b => new
                {
                    bookingId = b!.BookingID,
                    roomId = b.RoomID,
                    checkInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
                    checkOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
                    status = b.Status
                })
                .ToList();

            var dataset = new
            {
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                hotels = availableHotels,
                rooms = availableRooms,
                recentBookings = upcomingBookings
            };

            var serializedDataset = JsonSerializer.Serialize(dataset, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var builder = new StringBuilder();
            builder.AppendLine("Bạn là trợ lý AI của hệ thống đặt phòng khách sạn. Nhiệm vụ của bạn là hỗ trợ người dùng tìm phòng phù hợp, giải đáp thắc mắc về khách sạn và chỉ đưa ra gợi ý dựa trên dữ liệu cung cấp.");
            builder.AppendLine("\nLuôn trả lời bằng tiếng Việt chuẩn mực. Bạn phải phản hồi theo cấu trúc JSON sau:");
            builder.AppendLine("{" +
                               "\"reply\": \"<nội dung trả lời hoặc câu hỏi để lấy thêm thông tin>\"," +
                               "\"recommendedRooms\": [" +
                               "{\"roomId\": \"<mã phòng>\", \"hotelId\": \"<mã khách sạn>\", \"hotelName\": \"<tên khách sạn>\", \"roomType\": \"<loại phòng>\", \"pricePerNight\": <giá mỗi đêm>, \"reason\": \"<lý do gợi ý hoặc tóm tắt ngắn gọn>\"}" +
                               "]" +
                               "}");
            builder.AppendLine("\nTrước khi đề xuất phòng, hãy đảm bảo đã biết thành phố hoặc điểm đến, ngày nhận phòng, ngày trả phòng, số lượng khách và ngân sách mong muốn. Nếu thiếu bất kỳ thông tin nào, hãy đặt câu hỏi trong trường reply và để mảng recommendedRooms trống.");
            builder.AppendLine("Khi người dùng yêu cầu tóm tắt hoặc thông tin cụ thể về phòng/khách sạn, hãy trả lời trong reply và chỉ đưa phòng vào recommendedRooms nếu thực sự phù hợp.");

            var missingInformationInstruction = BuildRoomSearchGuidance(userMessage, availableHotels.Select(h => h.city ?? string.Empty));
            if (!string.IsNullOrWhiteSpace(missingInformationInstruction))
            {
                builder.AppendLine();
                builder.AppendLine(missingInformationInstruction);
            }

            builder.AppendLine("\nDữ liệu khách sạn và phòng (định dạng JSON):");
            builder.AppendLine(serializedDataset);

            return builder.ToString();
        }

        private object BuildPayload(string systemPrompt, IReadOnlyCollection<AIChat> history, string message)
        {
            var contents = new List<object>();

            foreach (var chat in history)
            {
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = chat.Message } }
                });

                if (!string.IsNullOrWhiteSpace(chat.Response))
                {
                    contents.Add(new
                    {
                        role = "model",
                        parts = new[] { new { text = chat.Response } }
                    });
                }
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = message } }
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
                    temperature = 0.3,
                    topP = 0.9,
                    topK = 40,
                    maxOutputTokens = _options.MaxOutputTokens
                }
            };
        }

        private static string BuildEndpoint(string apiKey, string model)
        {
            var baseUrl = DefaultApiBaseUrl.TrimEnd('/');
            var modelName = string.IsNullOrWhiteSpace(model) ? "gemini-2.0-flash" : model;
            return $"{baseUrl}/models/{modelName}:generateContent?key={apiKey}";
        }

        private AIChat CreateFallbackChat(string userId, string message, string response)
        {
            return new AIChat
            {
                ChatID = Guid.NewGuid().ToString(),
                UserID = userId,
                Message = message,
                Response = response,
                CreatedAt = DateTime.UtcNow
            };
        }

        private string? ResolveApiKey()
        {
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return _options.ApiKey;
            }

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                         ?? Environment.GetEnvironmentVariable("GEMINI__APIKEY")
                         ?? Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _options.ApiKey = apiKey.Trim();
                return _options.ApiKey;
            }

            _logger.LogWarning("Gemini API key is not configured. Please set GEMINI_API_KEY environment variable.");
            return null;
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
            return exception is HttpRequestException or TaskCanceledException;
        }

        private static TimeSpan? GetServerSuggestedDelay(HttpResponseMessage response)
        {
            var retryAfter = response.Headers.RetryAfter;
            if (retryAfter == null)
            {
                return null;
            }

            if (retryAfter.Delta is { } delta && delta > TimeSpan.Zero)
            {
                return delta;
            }

            if (retryAfter.Date is { } date)
            {
                var deltaFromDate = date - DateTimeOffset.UtcNow;
                if (deltaFromDate > TimeSpan.Zero)
                {
                    return deltaFromDate;
                }
            }

            return null;
        }

        private static TimeSpan CalculateNextDelay(TimeSpan currentDelay, TimeSpan? serverSuggestedDelay)
        {
            const double backoffMultiplier = 2.0;
            var maxDelay = TimeSpan.FromSeconds(10);

            TimeSpan baseDelay;
            if (serverSuggestedDelay is { } suggested && suggested > TimeSpan.Zero)
            {
                baseDelay = suggested;
            }
            else
            {
                var nextSeconds = Math.Min(currentDelay.TotalSeconds * backoffMultiplier, maxDelay.TotalSeconds);
                baseDelay = TimeSpan.FromSeconds(nextSeconds);
            }

            var jitterMilliseconds = Random.Shared.Next(250, 750);
            var candidate = baseDelay + TimeSpan.FromMilliseconds(jitterMilliseconds);

            return candidate <= maxDelay ? candidate : maxDelay;
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

        private AiAssistantResponse ParseModelResponse(string text)
        {
            var raw = text.Trim();
            var cleaned = StripCodeFence(raw);

            if (TryParsePayload(cleaned, raw, out var parsedResponse))
            {
                return parsedResponse!;
            }

            var extractedJson = ExtractFirstJsonObject(cleaned);
            if (extractedJson != null && TryParsePayload(extractedJson, raw, out parsedResponse))
            {
                return parsedResponse!;
            }

            var friendlyReply = ExtractReplyText(cleaned) ??
                                 "Xin lỗi, mình chưa đọc được phản hồi phù hợp. Bạn thử hỏi lại giúp mình nhé?";

            _logger.LogWarning("Không thể phân tích phản hồi JSON từ Gemini: {Text}", cleaned);

            return new AiAssistantResponse
            {
                Reply = friendlyReply,
                Recommendations = new List<AiRecommendation>(),
                RawText = raw
            };
        }

        private static bool TryParsePayload(string json, string raw, out AiAssistantResponse? response)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<GeminiChatPayload>(json, JsonOptions);
                if (payload == null)
                {
                    response = null;
                    return false;
                }

                var reply = string.IsNullOrWhiteSpace(payload.Reply)
                    ? ExtractReplyText(json) ?? raw
                    : payload.Reply.Trim();

                var recommendations = payload.RecommendedRooms?
                        .Where(room => room != null)
                        .Select(room => new AiRecommendation
                        {
                            RoomId = room!.RoomId,
                            HotelId = room.HotelId,
                            HotelName = room.HotelName,
                            RoomType = room.RoomType,
                            PricePerNight = room.PricePerNight,
                            Reason = room.Reason
                        })
                        .ToList()
                    ?? new List<AiRecommendation>();

                response = new AiAssistantResponse
                {
                    Reply = reply,
                    Recommendations = recommendations,
                    RawText = raw
                };

                return true;
            }
            catch (JsonException)
            {
                response = null;
                return false;
            }
        }

        private string BuildDisplayResponse(AiAssistantResponse response, IDictionary<string, Hotel> hotels, IDictionary<string, Room> rooms)
        {
            var builder = new StringBuilder();
            builder.AppendLine(response.Reply.Trim());

            if (response.Recommendations.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Gợi ý phòng phù hợp:");

                foreach (var recommendation in response.Recommendations)
                {
                    var details = new List<string>();

                    if (!string.IsNullOrWhiteSpace(recommendation.RoomId) && rooms.TryGetValue(recommendation.RoomId, out var room))
                    {
                        if (hotels.TryGetValue(room.HotelID, out var hotel))
                        {
                            details.Add($"Khách sạn {hotel.HotelName}");
                        }

                        details.Add($"Phòng {room.RoomNumber} ({room.RoomType})");
                        details.Add($"Sức chứa {room.Capacity} khách");
                        details.Add($"Giá {FormatCurrency(room.PricePerNight)} mỗi đêm");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(recommendation.HotelName))
                        {
                            details.Add($"Khách sạn {recommendation.HotelName}");
                        }

                        if (!string.IsNullOrWhiteSpace(recommendation.RoomType))
                        {
                            details.Add(recommendation.RoomType);
                        }

                        if (recommendation.PricePerNight.HasValue)
                        {
                            details.Add($"Giá khoảng {FormatCurrency(recommendation.PricePerNight.Value)} mỗi đêm");
                        }
                    }

                    var line = new StringBuilder("- ");
                    line.Append(details.Any() ? string.Join(", ", details) : "Tùy chọn");

                    if (!string.IsNullOrWhiteSpace(recommendation.Reason))
                    {
                        line.Append($" – {recommendation.Reason.Trim()}");
                    }

                    builder.AppendLine(line.ToString());
                }
            }

            if (!string.Equals(response.RawText, builder.ToString(), StringComparison.Ordinal))
            {
                _logger.LogDebug("Gemini raw response: {Raw}", response.RawText);
            }

            return builder.ToString().TrimEnd();
        }

        private static string FormatCurrency(double value)
        {
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} VND", value);
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

        private static string? ExtractFirstJsonObject(string text)
        {
            var start = text.IndexOf('{');
            if (start < 0)
            {
                return null;
            }

            var depth = 0;
            var inString = false;
            var escape = false;

            for (var i = start; i < text.Length; i++)
            {
                var ch = text[i];

                if (inString)
                {
                    if (escape)
                    {
                        escape = false;
                    }
                    else if (ch == '\\')
                    {
                        escape = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    depth++;
                    if (depth == 1)
                    {
                        start = i;
                    }
                }
                else if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text[start..(i + 1)];
                    }
                }
            }

            return null;
        }

        private static string? ExtractReplyText(string text)
        {
            const string key = "\"reply\"";
            var index = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return null;
            }

            var colonIndex = text.IndexOf(':', index + key.Length);
            if (colonIndex < 0)
            {
                return null;
            }

            var quoteIndex = text.IndexOf('"', colonIndex + 1);
            if (quoteIndex < 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            var escaped = false;

            for (var i = quoteIndex + 1; i < text.Length; i++)
            {
                var ch = text[i];

                if (escaped)
                {
                    builder.Append(ch switch
                    {
                        '"' => '"',
                        '\\' => '\\',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => ch
                    });
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    return builder.ToString().Trim();
                }

                builder.Append(ch);
            }

            return null;
        }

        private sealed class AiAssistantResponse
        {
            public string Reply { get; set; } = string.Empty;
            public List<AiRecommendation> Recommendations { get; set; } = new();
            public string RawText { get; set; } = string.Empty;
        }

        private sealed class AiRecommendation
        {
            public string? RoomId { get; set; }
            public string? HotelId { get; set; }
            public string? HotelName { get; set; }
            public string? RoomType { get; set; }
            public double? PricePerNight { get; set; }
            public string? Reason { get; set; }
        }

        private sealed class GeminiChatPayload
        {
            [JsonPropertyName("reply")]
            public string? Reply { get; set; }

            [JsonPropertyName("recommendedRooms")]
            public List<GeminiRecommendedRoom?>? RecommendedRooms { get; set; }
        }

        private sealed class GeminiRecommendedRoom
        {
            [JsonPropertyName("roomId")]
            public string? RoomId { get; set; }

            [JsonPropertyName("hotelId")]
            public string? HotelId { get; set; }

            [JsonPropertyName("hotelName")]
            public string? HotelName { get; set; }

            [JsonPropertyName("roomType")]
            public string? RoomType { get; set; }

            [JsonPropertyName("pricePerNight")]
            public double? PricePerNight { get; set; }

            [JsonPropertyName("reason")]
            public string? Reason { get; set; }
        }
     }
 }
