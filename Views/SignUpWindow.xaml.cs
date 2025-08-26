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
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Window
    {
        ISignUpViewModel _signupViewModel = App.Provider.GetRequiredService<ISignUpViewModel>();
        public SignUpWindow()
        {
            InitializeComponent();
            DataContext = _signupViewModel;
            txtConfirmedPassword.PasswordChanged += (s, e) => { _signupViewModel.PasswordConfirmed = txtConfirmedPassword.Password; };
            txtPassword.PasswordChanged += (s, e) => { _signupViewModel.Passoword = txtPassword.Password; };
            
        }
    }
}
