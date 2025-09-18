using System.Windows;
using Hotel_Booking_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        private readonly HotelAdminViewModel _hotelAdminViewModel = App.Provider.GetRequiredService<HotelAdminViewModel>();

        public HotelAdminWindow()
        {
            InitializeComponent();
            DataContext = _hotelAdminViewModel;

            txtCurrentPassword.PasswordChanged += (s, e) =>
            {
                _hotelAdminViewModel.CurrentPassword = txtCurrentPassword.Password;
            };

            txtNewPassword.PasswordChanged += (s, e) =>
            {
                _hotelAdminViewModel.NewPassword = txtNewPassword.Password;
            };

            txtConfirmPassword.PasswordChanged += (s, e) =>
            {
                _hotelAdminViewModel.ConfirmPassword = txtConfirmPassword.Password;
            };

            Loaded += async (s, e) => await _hotelAdminViewModel.LoadReviewsAsync();
        }
    }
}
