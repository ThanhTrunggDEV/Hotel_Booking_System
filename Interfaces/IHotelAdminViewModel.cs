using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    public interface IHotelAdminViewModel
    {
        ObservableCollection<Review> Reviews { get; }
        User CurrentUser { get; set; }
        Task LoadReviewsAsync();
    }
}
