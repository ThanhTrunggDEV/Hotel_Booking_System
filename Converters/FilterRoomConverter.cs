using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
    class FilterRoomConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double? price = null;
            if (double.TryParse(values[0]?.ToString(), out var p))
                price = p;

            int? capacity = null;
            if (int.TryParse(values[1]?.ToString(), out var cap))
                capacity = cap;

            var filterDict = new Dictionary<string, object?>
            {
                { "Price", price },
                { "Capacity", capacity }
            };

            return filterDict;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
