using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
    partial class ForgotPasswordViewModel : Bindable, IForgotPasswordViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserRepository _userRepository;
        private readonly IAuthentication _authentication;

        public ForgotPasswordViewModel(INavigationService navigationService, IUserRepository userRepository, IAuthentication authentication)
        {
            _authentication = authentication;
            _navigationService = navigationService;
            _userRepository = userRepository;
        }

        private string stepOneStatus = "Visible";
        private string stepTwoStatus = "Hidden";
        private string stepThreeStatus = "Hidden";

        private string OTP = "";
        public string UserEnteredOTP {  get; set; }
        public string UserEmail { get; set; }

        public string StepOneStatus { get => stepOneStatus; set => Set(ref stepOneStatus, value); } 
        public string StepTwoStatus { get => stepTwoStatus; set => Set(ref stepTwoStatus, value); } 
        public string StepThreeStatus { get => stepThreeStatus; set => Set(ref stepThreeStatus, value); } 

        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }

        private User? CurrentUser;

        [RelayCommand]
        private async Task SendOTP()
        {
            CurrentUser = await _userRepository.GetByEmailAsync(UserEmail); 
            if(CurrentUser == null)
            {
                MessageBox.Show("Email Chưa Đăng Ký Vui Lòng Kiểm Tra Lại");
                return;
            }
            
            OTP = new Random().Next(100000, 999999).ToString();
            bool isSent = await MailService.SendOTP(OTP, UserEmail);
            if(isSent)
            {
                StepOneStatus = "Hidden";
                StepTwoStatus = "Visible";
            }
            

        }
        [RelayCommand]
        private void ConfirmOTP()
        {
            if(OTP == UserEnteredOTP)
            {
                StepTwoStatus = "Hidden";
                StepThreeStatus = "Visible";
            }
            else
            {
                MessageBox.Show("Sai OTP vui lòng kiểm tra lại");
            }
            
        }
        [RelayCommand]
        private void ResetPassword()
        {
            if(NewPassword.Length >= 6 && NewPassword == NewPasswordConfirm)
            {
                string hashedPassword = _authentication.HashPassword(NewPassword);
                CurrentUser.Password = hashedPassword;
                _userRepository.UpdateAsync(CurrentUser);
                _navigationService.NavigateToLogin();
            }
            else
            {
                MessageBox.Show("Mật khẩu không khớp hoặc quá ngắn vui  lòng kiểm tra lại");
            }
        }
    }
}
