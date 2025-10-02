using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
                        var text = ExtractText(document, out var noTextReason);

                        if (string.IsNullOrWhiteSpace(text))
                        {
                            var fallbackMessage = noTextReason ??
                                                  "Mình chưa nhận được câu trả lời từ AI. Bạn có thể thử hỏi lại giúp mình không?";

                            if (noTextReason != null)
                            {
                                _logger.LogWarning("Gemini API không trả về nội dung văn bản: {Reason}", noTextReason);
                            }
                            else
                            {
                                _logger.LogWarning("Gemini API không trả về nội dung văn bản.");
                            }

                            return CreateFallbackChat(userId, message, fallbackMessage);
                        }

                        var modelResponse = ParseModelResponse(text);
                        var displayResponse = BuildDisplayResponse(modelResponse);

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
            builder.AppendLine("Bạn là trợ lý AI của hệ thống đặt phòng khách sạn. Hãy hỗ trợ người dùng tìm phòng, giải đáp thắc mắc về khách sạn và chỉ đưa ra gợi ý dựa trên dữ liệu cung cấp.");
            builder.AppendLine("\nLuôn trả lời bằng tiếng Việt tự nhiên, không sử dụng định dạng JSON.");
            builder.AppendLine("Trước khi đề xuất phòng, bạn phải hỏi và xác nhận từng thông tin quan trọng theo đúng thứ tự: (1) thành phố hoặc địa điểm, (2) ngày nhận phòng, (3) ngày trả phòng, (4) số lượng khách, (5) ngân sách hoặc mức giá mong muốn. Chỉ chuyển bước khi đã có câu trả lời rõ ràng cho bước hiện tại. Nếu người dùng đã cung cấp một vài thông tin, hãy xác nhận lại và tiếp tục hỏi những mục còn thiếu.");
            builder.AppendLine("Khi đã có đủ dữ liệu, hãy tóm tắt lại thông tin hiểu được và gợi ý tối đa vài khách sạn/phòng phù hợp. Nêu rõ tên khách sạn, loại phòng (nếu có) và lý do ngắn gọn vì sao nên chọn. Nếu người dùng yêu cầu tóm tắt hoặc thông tin cụ thể, hãy giải thích trực tiếp trong câu trả lời.");
            builder.AppendLine("Nếu chưa có lựa chọn phù hợp, hãy nói rõ lý do và hướng dẫn người dùng điều chỉnh yêu cầu (ví dụ thay đổi ngày, ngân sách hoặc vị trí). Không được trả lời chung chung là không có phản hồi.");

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

        private string BuildEndpoint(string apiKey, string? model)
        {
            var baseUrl = DefaultApiBaseUrl.TrimEnd('/');
            var resolvedModel = string.IsNullOrWhiteSpace(model) ? _options.DefaultModel : model;

            if (string.IsNullOrWhiteSpace(resolvedModel))
            {
                resolvedModel = "gemini-2.0-flash";
            }

            return $"{baseUrl}/models/{resolvedModel}:generateContent?key={apiKey}";
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

        private static string? ExtractText(JsonDocument document, out string? explanation)
        {
            explanation = null;

            if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array)
            {
                TryBuildPromptFeedbackMessage(document.RootElement, out explanation);
                return null;
            }

            foreach (var candidate in candidates.EnumerateArray())
            {
                if (TryExtractTextFromCandidate(candidate, out var text, out var candidateExplanation))
                {
                    return text;
                }

                if (explanation == null && !string.IsNullOrWhiteSpace(candidateExplanation))
                {
                    explanation = candidateExplanation;
                }

                if (explanation == null && candidate.TryGetProperty("finishReason", out var finishReasonElement))
                {
                    var finishReason = finishReasonElement.GetString();
                    if (string.Equals(finishReason, "SAFETY", StringComparison.OrdinalIgnoreCase))
                    {
                        explanation = "Xin lỗi, yêu cầu vừa rồi bị hệ thống AI chặn vì lý do an toàn. Bạn hãy thử diễn đạt lại nội dung một cách khác nhé.";
                    }
                }

                if (explanation == null && candidate.TryGetProperty("safetyRatings", out var safetyRatings) && safetyRatings.ValueKind == JsonValueKind.Array)
                {
                    explanation = BuildSafetyRatingsMessage(safetyRatings);
                }
            }

            if (explanation == null)
            {
                TryBuildPromptFeedbackMessage(document.RootElement, out explanation);
            }

            return null;
        }

        private AiAssistantResponse ParseModelResponse(string text)
        {
            var raw = text.Trim();
            var cleaned = StripCodeFence(raw);

            return new AiAssistantResponse
            {
                Reply = string.IsNullOrWhiteSpace(cleaned) ? raw : cleaned,
                RawText = raw
            };
        }

        private static bool TryExtractTextFromCandidate(JsonElement candidate, out string? text, out string? explanation)
        {
            text = null;
            explanation = null;

            if (!candidate.TryGetProperty("content", out var content))
            {
                return false;
            }

            if (TryExtractTextFromContent(content, out text, out explanation))
            {
                return true;
            }

            return false;
        }

        private static bool TryExtractTextFromContent(JsonElement content, out string? text, out string? explanation)
        {
            text = null;
            explanation = null;

            JsonElement partsElement;
            if (content.ValueKind == JsonValueKind.Array)
            {
                partsElement = content;
            }
            else if (content.ValueKind == JsonValueKind.Object && content.TryGetProperty("parts", out var extractedParts))
            {
                partsElement = extractedParts;
            }
            else
            {
                return false;
            }

            return TryExtractTextFromParts(partsElement, out text, out explanation);
        }

        private static bool TryExtractTextFromParts(JsonElement parts, out string? text, out string? explanation)
        {
            text = null;
            explanation = null;

            if (parts.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (part.TryGetProperty("text", out var textElement))
                {
                    var candidate = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        text = candidate;
                        return true;
                    }
                }

                if (part.TryGetProperty("inlineData", out var inlineData) && inlineData.ValueKind == JsonValueKind.Object)
                {
                    var decoded = TryDecodeInlineData(inlineData);
                    if (!string.IsNullOrWhiteSpace(decoded))
                    {
                        text = decoded;
                        return true;
                    }
                }

                if (part.TryGetProperty("functionCall", out var functionCall) && functionCall.ValueKind == JsonValueKind.Object)
                {
                    if (TryExtractFunctionCallArguments(functionCall, out var functionCallText))
                    {
                        text = functionCallText;
                        return true;
                    }

                    if (explanation == null)
                    {
                        var name = functionCall.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                        explanation = BuildFunctionCallExplanation(name);
                    }
                }
            }

            return false;
        }

        private static bool TryExtractFunctionCallArguments(JsonElement functionCall, out string? text)
        {
            text = null;

            if (functionCall.TryGetProperty("args", out var args))
            {
                if (TryNormalizeFunctionValue(args, out text))
                {
                    return true;
                }
            }

            if (functionCall.TryGetProperty("arguments", out var arguments))
            {
                if (TryNormalizeFunctionValue(arguments, out text))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryNormalizeFunctionValue(JsonElement element, out string? text)
        {
            text = null;

            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        text = value;
                        return true;
                    }

                    break;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    var raw = element.GetRawText();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        text = raw;
                        return true;
                    }

                    break;
            }

            return false;
        }

        private static string? TryDecodeInlineData(JsonElement inlineData)
        {
            if (!inlineData.TryGetProperty("data", out var dataElement))
            {
                return null;
            }

            var base64 = dataElement.GetString();
            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static string BuildFunctionCallExplanation(string? functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return "Gemini đang cố gắng gọi một công cụ và không trả về câu trả lời dạng văn bản. Bạn hãy thử mô tả yêu cầu chi tiết hơn hoặc đặt lại câu hỏi nhé.";
            }

            return $"Gemini đang cố gắng gọi công cụ \"{functionName}\" nên không trả về văn bản. Bạn hãy thử đặt lại câu hỏi hoặc cung cấp thêm chi tiết cụ thể hơn nhé.";
        }

        private static string? BuildSafetyRatingsMessage(JsonElement safetyRatings)
        {
            if (safetyRatings.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var blockedCategories = new List<string>();

            foreach (var rating in safetyRatings.EnumerateArray())
            {
                if (rating.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!rating.TryGetProperty("blocked", out var blockedElement) || blockedElement.ValueKind != JsonValueKind.True)
                {
                    continue;
                }

                if (rating.TryGetProperty("category", out var categoryElement))
                {
                    var category = categoryElement.GetString();
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        blockedCategories.Add(category);
                    }
                }
            }

            if (blockedCategories.Count == 0)
            {
                return null;
            }

            var categories = string.Join(", ", blockedCategories);
            return $"Xin lỗi, câu hỏi vừa rồi bị hệ thống AI chặn vì liên quan tới nhóm nội dung: {categories}. Bạn hãy thử diễn đạt lại hoặc thay đổi yêu cầu nhé.";
        }

        private static bool TryBuildPromptFeedbackMessage(JsonElement root, out string? message)
        {
            message = null;

            if (!root.TryGetProperty("promptFeedback", out var feedback) || feedback.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (feedback.TryGetProperty("blockReason", out var blockReasonElement))
            {
                var blockReason = blockReasonElement.GetString();
                if (!string.IsNullOrWhiteSpace(blockReason))
                {
                    message = blockReason switch
                    {
                        "SAFETY" => "Gemini đã từ chối trả lời vì nội dung có thể vi phạm chính sách an toàn. Bạn hãy thử mô tả yêu cầu khác đi nhé.",
                        "OTHER" => "Gemini không thể trả lời yêu cầu này. Bạn hãy thử đặt câu hỏi khác hoặc đơn giản hơn.",
                        _ => "Gemini hiện chưa thể trả lời yêu cầu này. Bạn hãy thử lại sau hoặc điều chỉnh câu hỏi nhé."
                    };
                    return true;
                }
            }

            if (feedback.TryGetProperty("safetyRatings", out var safetyRatings) && safetyRatings.ValueKind == JsonValueKind.Array)
            {
                message = BuildSafetyRatingsMessage(safetyRatings);
                return !string.IsNullOrWhiteSpace(message);
            }

            return false;
        }

        private string BuildDisplayResponse(AiAssistantResponse response)
        {
            var reply = response.Reply.Trim();

            if (!string.Equals(response.RawText, reply, StringComparison.Ordinal))
            {
                _logger.LogDebug("Gemini raw response: {Raw}", response.RawText);
            }

            return string.IsNullOrWhiteSpace(reply) ? response.RawText : reply;
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

        private sealed class AiAssistantResponse
        {
            public string Reply { get; set; } = string.Empty;
            public string RawText { get; set; } = string.Empty;
        }
     }
 }
