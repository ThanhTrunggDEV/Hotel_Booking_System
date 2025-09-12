using System;
using System.Threading.Tasks;
using Mscc.GenerativeAI;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;
        private readonly GeminiOptions _options;

        public AIChatService(IAIChatRepository repository, GeminiOptions options)
        {
            _repository = repository;
            _options = options;
        }

        public async Task<AIChat> SendAsync(string userId, string message, string? model = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            var googleAI = new GoogleAI(apiKey: _options.ApiKey);
            var generativeModel = googleAI.GenerativeModel(model ?? _options.DefaultModel);
            var result = await generativeModel.GenerateContentAsync(message);
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
