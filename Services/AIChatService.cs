using System;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;
        private readonly string _apiKey;
        private readonly string _model;

        public AIChatService(IAIChatRepository repository)
        {
            _repository = repository;
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? throw new InvalidOperationException("GEMINI_API_KEY is not set");
            _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-pro";
        }

        public async Task<AIChat> SendAsync(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            var response = await GetResponseFromAIAsync(message);

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

        private async Task<string> GetResponseFromAIAsync(string message)
        {
            // Simulated call to Gemini API using the configured model.
            await Task.Delay(500);
            return $"AI ({_model}) response: {message}";
        }
    }
}
