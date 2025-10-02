namespace Hotel_Booking_System.Services
{
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gemini-2.5-flash";
        public string ApiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
        public int MaxOutputTokens { get; set; } = 1024;
    }
}
