using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hotel_Booking_System.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "available":
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80));   // xanh lá
                    case "booked":
                        return new SolidColorBrush(Color.FromRgb(244, 67, 54));  // đỏ
                    case "pending":
                        return new SolidColorBrush(Color.FromRgb(255, 193, 7));  // vàng
                    default:
                        return new SolidColorBrush(Colors.Gray); // fallback
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
