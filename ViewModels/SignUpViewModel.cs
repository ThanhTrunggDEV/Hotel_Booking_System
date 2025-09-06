using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
    partial class SignUpViewModel : ISignUpViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        private readonly IAuthentication _authentication;

        private string OTP = "";
        private bool _isOTPSent = false;

        public SignUpViewModel(IUserRepository userRepository, INavigationService navigationService, IAuthentication authentication)
        {
            _userRepository = userRepository;
            _navigationService = navigationService;
            _authentication = authentication;
            
        }



        public string Passoword { get; set; }
        public string PasswordConfirmed { get; set; }

        [RelayCommand(CanExecute = nameof(CanSendOTP))]
        private async Task SendOTP(string userEmail)
        {
            if (!userEmail.EndsWith("@gmail.com"))
            {
                MessageBox.Show("Email sai định dạng vui lòng kiểm tra lại");
                return;
            }
            var isExisted = await _userRepository.GetByEmailAsync(userEmail);
            if (isExisted != null)
            {
                MessageBox.Show("Email đã được đăng ký vui lòng kiểm tra lại");
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
            if(Passoword != PasswordConfirmed)
            {
                MessageBox.Show("Mật khẩu không khớp vui lòng kiểm tra lại");
                return;
            }

            Tuple<User, string>? data = para as Tuple<User, string>;

            if (data!.Item2 != OTP)
            {
                MessageBox.Show("OTP không khớp vui lòng kiểm tra lại");
                return;
            }
            try
            {
                string hashedPassword = _authentication.HashPassword(Passoword);
                data.Item1.Password = hashedPassword;
                data.Item1.Role = "User";

                await _userRepository.AddAsync(data.Item1);
                await _userRepository.SaveAsync();
                MessageBox.Show("Tạo tài khoản thành công");
            }
            catch
            {
                MessageBox.Show("Tạo thật bại");
            }

        }
        [RelayCommand]
        private void BackToLogin()
        {
            _navigationService.NavigateToLogin();
        }

    }
}
