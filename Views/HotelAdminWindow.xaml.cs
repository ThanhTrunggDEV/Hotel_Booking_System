using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        private readonly IHotelAdminViewModel _hotelAdminViewModel = App.Provider.GetRequiredService<IHotelAdminViewModel>();

        public HotelAdminWindow()
        {
            InitializeComponent();
            DataContext = _hotelAdminViewModel;

            txtCurrentPassword.PasswordChanged += (s, e) =>
            {
                (_hotelAdminViewModel as dynamic).CurrentPassword = txtCurrentPassword.Password;
            };

            txtNewPassword.PasswordChanged += (s, e) =>
            {
                (_hotelAdminViewModel as dynamic).NewPassword = txtNewPassword.Password;
            };

            txtConfirmPassword.PasswordChanged += (s, e) =>
            {
                (_hotelAdminViewModel as dynamic).ConfirmPassword = txtConfirmPassword.Password;
            };

            Loaded += async (s, e) => await _hotelAdminViewModel.LoadReviewsAsync();
        }
    }
}
