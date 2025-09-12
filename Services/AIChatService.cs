using System;
using System.Threading.Tasks;
using Google.AI.GenerativeAI;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly IAIChatRepository _repository;
        private readonly GeminiOptions _options;
        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly string _apiKey;
        private readonly string _model;

        public AIChatService(IAIChatRepository repository, HttpClient httpClient, GeminiOptions options)
        {
            _repository = repository;
            _httpClient = httpClient;
            _options = options;
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                      ?? throw new InvalidOperationException("GEMINI_API_KEY is not set");
            _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-pro";
        }

        public async Task<AIChat> SendAsync(string userId, string message, string? model = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            var generativeModel = new GenerativeModel(model ?? _options.DefaultModel, _options.ApiKey);
            var result = await generativeModel.GenerateContentAsync(message);
            var response = await GetResponseFromAIAsync(message, model);

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


        private async Task<string> GetResponseFromAIAsync(string message, string model)
        {
            var modelToUse = string.IsNullOrWhiteSpace(model) ? _options.DefaultModel : model;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelToUse}:generateContent?key={_options.ApiKey}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = message } }
                    }
                }
            };

            var httpResponse = await _httpClient.PostAsJsonAsync(url, request);
            httpResponse.EnsureSuccessStatusCode();

            using var stream = await httpResponse.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<GeminiResponse>(stream);
            var text = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
            return text ?? string.Empty;
        }

        private class GeminiResponse
        {
            public Candidate[] candidates { get; set; }
        }

        private class Candidate
        {
            public Content content { get; set; }
        }

        private class Content
        {
            public Part[] parts { get; set; }
        }

        private class Part
        {
            public string text { get; set; }
            // Simulated call to Gemini API using the configured model.
            await Task.Delay(500);
            return $"AI ({_model}) response: {message}";
        }

    }
}
