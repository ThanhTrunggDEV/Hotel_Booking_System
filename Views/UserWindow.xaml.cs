using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        IUserViewModel _userViewModel = App.Provider.GetRequiredService<IUserViewModel>();
        public UserWindow()
        {
            InitializeComponent();
            DataContext = _userViewModel;

            txtCurrentPassword.PasswordChanged += (s, e) =>
            {
                (_userViewModel as dynamic).CurrentPassword = txtCurrentPassword.Password;
            };
            txtNewPassword.PasswordChanged += (s, e) =>
            {
                (_userViewModel as dynamic).NewPassword = txtNewPassword.Password;
            };
            txtConfirmPassword.PasswordChanged += (s, e) =>
            {
                (_userViewModel as dynamic).ConfirmPassword = txtConfirmPassword.Password;
            };

        }

        
    }
}