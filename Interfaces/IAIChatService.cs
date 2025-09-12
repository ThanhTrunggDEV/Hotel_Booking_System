using Hotel_Booking_System.DomainModels;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    public interface IAIChatService
    {
        Task<AIChat> SendAsync(string userId, string message, string? model = null);
    }
}
