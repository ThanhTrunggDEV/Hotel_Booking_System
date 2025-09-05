using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
  
    class DateDiffConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return string.Empty;

            if (values[0] is DateTime checkin && values[1] is DateTime checkout)
            {
                if (checkout <= checkin)
                    return "0 days";

                TimeSpan diff = checkout - checkin;

               
                string result = "";
                if (diff.Days > 0)
                    result += $"{diff.Days} day{(diff.Days > 1 ? "s" : "")} ";

                if (diff.Hours > 0)
                    result += $"{diff.Hours} hour{(diff.Hours > 1 ? "s" : "")}";

                return string.IsNullOrWhiteSpace(result) ? "Less than 1 hour" : result.Trim();
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
