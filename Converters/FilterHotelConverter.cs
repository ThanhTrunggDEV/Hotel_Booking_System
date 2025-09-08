using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
    class FilterHotelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string location = values[0]?.ToString() ?? string.Empty;

            double? minPrice = null;
            if (double.TryParse(values[1]?.ToString(), out var min))
                minPrice = min;

            double? maxPrice = null;
            if (double.TryParse(values[2]?.ToString(), out var max))
                maxPrice = max;

            bool? fiveStar = values[3] as bool?;
            bool? fourStar = values[4] as bool?;
            bool? threeStar = values[5] as bool?;
            bool? twoStar = values[6] as bool?;
            bool? oneStar = values[7] as bool?;

            bool? freeWifi = values[8] as bool?;
            bool? swimmingPool = values[9] as bool?;
            bool? freeParking = values[10] as bool?;
            bool? restaurant = values[11] as bool?;
            bool? gym = values[12] as bool?;

            var filterDict = new Dictionary<string, object?>
            {
                { "Location", location },
                { "MinPrice", minPrice },
                { "MaxPrice", maxPrice },
                { "FiveStar", fiveStar },
                { "FourStar", fourStar },
                { "ThreeStar", threeStar },
                { "TwoStar", twoStar },
                { "OneStar", oneStar },
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
