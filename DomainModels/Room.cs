using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Room : INotifyPropertyChanged
    {
        [Key]
        public string RoomID { get; set; } = null!;
        public string HotelID { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;

        private string _roomImage = string.Empty;
        public string RoomImage
        {
            get => _roomImage;
            set
            {
                if (_roomImage != value)
                {
                    _roomImage = value;
                    PropertyChanged?.Invoke(this, new(nameof(RoomImage)));
                }
            }
        }

        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public double PricePerNight { get; set; }
        public string Status { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
