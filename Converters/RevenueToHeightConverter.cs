using System;
using System.Globalization;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
    public class RevenueToHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return 0d;
            }

            if (values[0] is not double amount || values[1] is not double maxAmount)
            {
                return 0d;
            }

            if (maxAmount <= 0)
            {
                return 0d;
            }

            var maxHeight = 120d;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedHeight))
            {
                maxHeight = parsedHeight;
            }

            var ratio = Math.Max(0, amount / maxAmount);
            return ratio * maxHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
