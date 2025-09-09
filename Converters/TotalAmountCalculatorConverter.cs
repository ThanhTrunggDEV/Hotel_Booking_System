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
    internal class TotalAmountCalculatorConverter : IMultiValueConverter
    {
        private readonly IRoomRepository _roomRepository = App.Provider!.GetRequiredService<IRoomRepository>();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string roomId = values[0] as string ?? "";
            string nightsText = values[1] as string ?? "0 nights";

            int nights = nightsText.Split(' ').FirstOrDefault() is string nightsStr && int.TryParse(nightsStr, out int n) ? n : 0;

            double pricePerNight = _roomRepository.GetByIdAsync(roomId).Result.PricePerNight;

            return (nights * pricePerNight).ToString("C0");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
