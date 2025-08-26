using Hotel_Booking_System.DomainModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Hotel_Booking_System.Converters
{
    class CreateAccountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string fullName = values[0]?.ToString() ?? "";
            string email = values[1]?.ToString() ?? "";
            string phone = values[2]?.ToString() ?? "";

            string otp = values[5]?.ToString() ?? string.Empty;

            DateTime dob = new();

            if (values[3] is DateTime dt)
            {
                dob = dt;
            }

            string gender = "";
            if (values[4] is ComboBoxItem item)
            {
                gender = item.Content + "";
            }

            return new Tuple<User, string>(new User { FullName = fullName, Email = email, Phone = phone, DateOfBirth = dob, Gender = gender }, otp);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
