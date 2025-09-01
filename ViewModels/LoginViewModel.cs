using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
    internal partial class LoginViewModel : Bindable ,ILoginViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthentication _authenticationService;
        private readonly IUserRepository _userRepository;

        private string email = "";
        private string password = "";
        private bool isSavedCredentials = false;
        public string Password { get => password; set => Set( ref password, value); }
        public string Email { get => email; set => Set( ref email, value ); }
        public bool IsSavedCredentials { get => isSavedCredentials; set => Set(ref isSavedCredentials, value); }

        
        public LoginViewModel(INavigationService navigationService, IAuthentication authenticationService, IUserRepository userRepository)
        {
            _authenticationService = authenticationService;
            _userRepository = userRepository;
            _navigationService = navigationService;


            LoadCredential();
        }
        [RelayCommand]
        private async Task Login(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
           
            if (user != null && user.Email == email && _authenticationService.VerifyPassword(Password, user.Password)  && user.Role == "User")
            {
                _navigationService.NavigateToUser();
                WeakReferenceMessenger.Default.Send(new MessageService(email));

                await SaveCredentials();
                
            }
            else if (user != null && user.Email == email && _authenticationService.VerifyPassword(Password, user.Password) && user.Role == "Admin")
            {
                WeakReferenceMessenger.Default.Send(new MessageService(email));
                _navigationService.NavigateToAdmin();
                await SaveCredentials();

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
        class AutoSave
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
            public bool RememberMe { get; set; }
        }
        private async Task SaveCredentials()
        {
            AutoSave dataSave = new AutoSave();
            if(IsSavedCredentials == false)
            {
                dataSave.Email = "";
                dataSave.Password = "";
                dataSave.RememberMe = false;
            }
            else
            {
                dataSave.Email = Email;
                dataSave.Password = Password;
                dataSave.RememberMe = true;
            }
            using (FileStream stream = File.Open("data.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<AutoSave>(stream, dataSave);
                
            }
        }
        private async Task LoadCredential()
        {
            using (FileStream stream = File.Open("data.json",FileMode.OpenOrCreate))
            {
                AutoSave? data = new();
                data = await JsonSerializer.DeserializeAsync<AutoSave>(stream);
                Email = data.Email ??= "";
                Password = data.Password ??= "";
                IsSavedCredentials = data.RememberMe;
            }
        }


    }
}
