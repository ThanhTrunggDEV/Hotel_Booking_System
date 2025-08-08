using CommunityToolkit.Mvvm.Input;
using Hotel_Manager.Interfaces;
using Hotel_Manager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Manager.ViewModels
{
    internal partial class LoginViewModel : ILoginViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserRepository _userRepository;
        public string Password { get; set; } = "";

        public LoginViewModel(INavigationService navigationService, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _navigationService = navigationService;
            
        }
        [RelayCommand]
        public async Task Login(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null && user.Username == username && Password == user.Password && user.Role == "User")
            {
                _navigationService.NavigateToUserWindow();
            }
            else if (user.Role == "Admin")
            {
                _navigationService.NavigateToAdminWindow();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
