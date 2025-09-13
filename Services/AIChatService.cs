using System.Text;
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

        public AIChatService(IAIChatRepository repository, GeminiOptions options, IBookingRepository bookingRepository, IHotelRepository hotelRepository, IRoomRepository roomRepository)
        {
            _repository = repository;
            _hotelRepository = hotelRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _options = options;

            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                         ?? throw new InvalidOperationException("GEMINI_API_KEY is not set");

            var modelName = Environment.GetEnvironmentVariable("GEMINI_MODEL")
                            ?? _options.DefaultModel
                            ?? Model.Gemini15Pro; 

         
            var googleAI = new GoogleAI(apiKey);
            _generativeModel = googleAI.GenerativeModel(model: modelName);
        }

        public async Task<AIChat> SendAsync(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            // 1. Lấy data từ DB
            var hotels = await _hotelRepository.GetAllAsync();
            var rooms = await _roomRepository.GetAllAsync();
            var bookings = await _bookingRepository.GetBookingByUserId(userId);

            // 2. Build context text
            var context = new StringBuilder();
            context.AppendLine("Here is the current hotel database snapshot:");
            context.AppendLine("\nHotels:");
            foreach (var h in hotels)
            {
                context.AppendLine($"- {h.HotelID}, {h.HotelName}, Location: {h.Address}, {h.City}, Price Range: {h.MinPrice}-{h.MaxPrice}, Hotel Rating: {h.Rating}");
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
            var prompt = $@"
                        You are an AI assistant for NTT hotel booking system.
                        Always answer based only on the given database information.
                        If the user asks something outside the data, politely say you don't know.

                      Database context: {context}
                      User question: {message}";

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

    }
}
