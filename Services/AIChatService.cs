using System;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;

        public AIChatService(IAIChatRepository repository)
        {
            _repository = repository;
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
            // Simulated call to AI API or internal module
            await Task.Delay(500);
            return $"AI response: {message}";
        }
    }
}
