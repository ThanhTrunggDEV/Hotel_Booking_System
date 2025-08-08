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
    internal class LoginViewModel : ILoginViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserRepository _userRepository;
        public string Password { get; set; } = "";

        public LoginViewModel(INavigationService navigationService, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _navigationService = navigationService;
            
        }
        
        public void Login(string username)
        {
            var user = _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            //_navigationService.NavigateToUserWindow();
            //_navigationService.NavigateToAdminWindow();
        }
    }
}
