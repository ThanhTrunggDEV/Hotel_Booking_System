
using System;

namespace Hotel_Booking_System.Services
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gemini-2.5-flash";
        public int MaxOutputTokens { get; set; } = 1024;
    }
}
