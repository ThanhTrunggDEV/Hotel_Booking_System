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

        private string _status = "Available";
        public string Status
        {
            get => _status;
            set
            {
                var normalized = NormalizeStatus(value);
                if (_status != normalized)
                {
                    _status = normalized;
                    PropertyChanged?.Invoke(this, new(nameof(Status)));
                }
            }
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.Equals(status, "Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                return "Maintenance";
            }

            return "Available";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
