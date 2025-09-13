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

            DateTime? checkIn = null;
            if (DateTime.TryParse(values[2]?.ToString(), out var ci))
                checkIn = ci;

            DateTime? checkOut = null;
            if (DateTime.TryParse(values[3]?.ToString(), out var co))
                checkOut = co;

            var filterDict = new Dictionary<string, object?>
            {
                { "Price", price },
                { "Capacity", capacity },
                { "CheckIn", checkIn },
                { "CheckOut", checkOut }
            };

            return filterDict;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
