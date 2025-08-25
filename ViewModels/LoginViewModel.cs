using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
    internal partial class LoginViewModel : ILoginViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthentication _authenticationService;
        private readonly IUserRepository _userRepository;

        public string Password { get; set; } = "";

        public LoginViewModel(INavigationService navigationService, IAuthentication authenticationService, IUserRepository userRepository)
        {
            _authenticationService = authenticationService;
            _userRepository = userRepository;
            _navigationService = navigationService;
            
        }
        [RelayCommand]
        private async Task Login(string username)
        {
            var user = await _userRepository.GetByEmailAsync(username);
            if (user != null && user.Email == username && _authenticationService.VerifyPassword(Password, user.Password)  && user.Role == "User")
            {
                _navigationService.NavigateToUser();
            }
            else if (user != null && user.Email == username && _authenticationService.VerifyPassword(Password, user.Password) && user.Role == "Admin")
            {
                _navigationService.NavigateToAdmin();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        [RelayCommand]
        private void ForgotPassword()
        {
            _navigationService.NavigationToForgotPassword();
        }
        [RelayCommand]
        private void SignUp()
        {
            _navigationService.NavigateToSignUp();
        }
        


    }
}
