using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        IForgotPasswordViewModel _forgotPasswordViewModel = App.Provider.GetRequiredService<IForgotPasswordViewModel>();
        public ForgotPasswordWindow()
        {
            InitializeComponent();
            DataContext = _forgotPasswordViewModel;

            txtPassword.PasswordChanged += (s, e) =>
            {
                _forgotPasswordViewModel.NewPassword = txtPassword.Password;
            };
            txtPasswordConfirmed.PasswordChanged += (s, e) =>
            {
                _forgotPasswordViewModel.NewPasswordConfirm = txtPasswordConfirmed.Password;
            };
            

        }
    }
}
