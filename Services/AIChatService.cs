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

        public AIChatService(IAIChatRepository repository, GeminiOptions options)
        {
            _repository = repository;
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

            var result = await _generativeModel.GenerateContent(message);
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
    }
}
