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
    internal class HotelIdToHotelNameConverter : IValueConverter
    {
        private IHotelRepository _hotelRepository = App.Provider!.GetRequiredService<IHotelRepository>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hotelId = value as string;
            if (string.IsNullOrEmpty(hotelId))
                return "Unknown Hotel";
            return _hotelRepository.GetByIdAsync(hotelId).Result?.HotelName ?? "Unknown Hotel";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
