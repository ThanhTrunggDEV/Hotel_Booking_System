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
            double? minPrice = null;
            if (double.TryParse(values[0]?.ToString(), out var min))
                minPrice = min;

            double? maxPrice = null;
            if (double.TryParse(values[1]?.ToString(), out var max))
                maxPrice = max;

            int? capacity = null;
            if (int.TryParse(values[2]?.ToString(), out var cap))
                capacity = cap;

            bool? freeWifi = values[3] as bool?;
            bool? swimmingPool = values[4] as bool?;
            bool? freeParking = values[5] as bool?;
            bool? restaurant = values[6] as bool?;
            bool? gym = values[7] as bool?;

            var filterDict = new Dictionary<string, object?>
            {
                { "MinPrice", minPrice },
                { "MaxPrice", maxPrice },
                { "Capacity", capacity },
                { "FreeWifi", freeWifi },
                { "SwimmingPool", swimmingPool },
                { "FreeParking", freeParking },
                { "Restaurant", restaurant },
                { "Gym", gym }
            };

            return filterDict;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
