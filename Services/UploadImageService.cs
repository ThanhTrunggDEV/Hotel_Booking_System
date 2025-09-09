using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.Services
{
    internal class UploadImageService
    {
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("IMAGE_API")!;
        public static async Task<string?> UploadAsync(string filePath)
        {
            try
            {
                var _httpClient = new HttpClient();
                using var form = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);

                
                form.Add(streamContent, "image", Path.GetFileName(filePath));

               
                var response = await _httpClient.PostAsync(
                    $"https://api.imgbb.com/1/upload?key={_apiKey}", form);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

            
                var root = System.Text.Json.JsonDocument.Parse(json);
                string url = root.RootElement
                    .GetProperty("data")
                    .GetProperty("url")
                    .GetString()!;

                return url;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Image upload failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return @"\Resources\avatar_default.png";
            }
        }
    }
}
