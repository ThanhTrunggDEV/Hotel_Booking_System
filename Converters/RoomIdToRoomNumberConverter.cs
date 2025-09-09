using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Converters
{
    internal class RoomIdToRoomNumberConverter : IValueConverter
    {
        private readonly IRoomRepository _roomRepository = App.Provider!.GetRequiredService<IRoomRepository>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string roomId = value as string;
            
            if (string.IsNullOrEmpty(roomId))
                return "Unknown Room";
            return _roomRepository.GetByIdAsync(roomId).Result?.RoomNumber ?? "Unknown Room";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
