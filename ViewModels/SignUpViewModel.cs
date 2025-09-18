using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Hotel_Booking_System.ViewModels
{
    partial class SignUpViewModel : Bindable, ISignUpViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        private readonly IAuthentication _authentication;

        private string OTP = "";
        private bool _isOTPSent = false;
        private string notificationMessage = "";
        private DispatcherTimer? _notificationTimer;

        public SignUpViewModel(IUserRepository userRepository, INavigationService navigationService, IAuthentication authentication)
        {
            _userRepository = userRepository;
            _navigationService = navigationService;
            _authentication = authentication;
            
        }



        public string Password { get; set; }
        public string PasswordConfirmed { get; set; }
        public string NotificationMessage { get => notificationMessage; set => Set(ref notificationMessage, value); }

        [RelayCommand(CanExecute = nameof(CanSendOTP))]
        private async Task SendOTP(string userEmail)
        {
            if (!userEmail.EndsWith("@gmail.com"))
            {
                ShowNotification("Email sai định dạng vui lòng kiểm tra lại");
                return;
            }
            var isExisted = await _userRepository.GetByEmailAsync(userEmail);
            if (isExisted != null)
            {
                ShowNotification("Email đã được đăng ký vui lòng kiểm tra lại");
                return;
            }
            OTP = new Random().Next(100000, 999999).ToString();
           bool isSent = await MailService.SendOTP(OTP, userEmail);


            if (isSent)
            {
                _isOTPSent = true;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                await Task.Run(() =>
                {
                    while (stopwatch.Elapsed.TotalSeconds < 60) ;
                });
                _isOTPSent = false;
            }
                

        }
        private bool CanSendOTP() => !_isOTPSent;
        [RelayCommand]
        private async Task CreateAccount(object para)
        {
            if (Password != PasswordConfirmed)
            {
                ShowNotification("Mật khẩu không khớp vui lòng kiểm tra lại");
                return;
            }

            Tuple<User, string>? data = para as Tuple<User, string>;

            if (data!.Item2 != OTP)
            {
                ShowNotification("OTP không khớp vui lòng kiểm tra lại");
                return;
            }
            try
            {
                string hashedPassword = _authentication.HashPassword(Password);
                data.Item1.Password = hashedPassword;
                data.Item1.Role = "User";

                await _userRepository.AddAsync(data.Item1);
                await _userRepository.SaveAsync();
                ShowNotification("Tạo tài khoản thành công");
            }
            catch
            {
                ShowNotification("Tạo thật bại");
            }

        }
        [RelayCommand]
        private void BackToLogin()
        {
            _navigationService.NavigateToLogin();
        }

        private void ShowNotification(string message)
        {
            NotificationMessage = message;
            _notificationTimer?.Stop();
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _notificationTimer.Tick += (s, e) =>
            {
                NotificationMessage = string.Empty;
                _notificationTimer.Stop();
            };
            _notificationTimer.Start();
        }

    }
}
