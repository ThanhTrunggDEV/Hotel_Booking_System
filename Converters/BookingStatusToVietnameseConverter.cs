using System;
using System.Globalization;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
    public class BookingStatusToVietnameseConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var status = value.ToString();

            return status switch
            {
                "All" => "Tất cả",
                "Pending" => "Đang chờ",
                "Confirmed" => "Đã xác nhận",
                "Cancelled" => "Đã hủy",
                "CancelledRequested" => "Yêu cầu hủy",
                "ModifyRequested" => "Yêu cầu chỉnh sửa",
                "Done" => "Hoàn tất",
                _ => status ?? string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
