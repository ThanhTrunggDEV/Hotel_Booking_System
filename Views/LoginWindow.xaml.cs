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
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ILoginViewModel _loginViewModel = App.Provider!.GetRequiredService<ILoginViewModel>();
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = _loginViewModel;
            txtPassword.PasswordChanged += (s, e) =>
            {
                _loginViewModel.Password = txtPassword.Password;    
            };
        }
    }
}
