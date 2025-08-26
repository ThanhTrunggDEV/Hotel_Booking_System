using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.Services
{
    class NavigationService : INavigationService
    {
        private void CloseCurrent()
        {
             App.Current.Windows[0].Close();
            //foreach (Window window in App.Current.Windows)
            //{
            //    if (window.IsVisible)
            //    {
            //        window.Hide();
            //    }
            //}
        }
        public void NavigateToAdmin()
        {
            
            App.Provider!.GetRequiredService<AdminWindow>().Show();
            CloseCurrent();
        }

        public void NavigateToUser()
        {
            
            App.Provider!.GetRequiredService<UserWindow>().Show();
            CloseCurrent();
        }

     
        public void NavigateToSignUp()
        {
            
            App.Provider!.GetRequiredService<SignUpWindow>().Show();
            CloseCurrent();

        }

        public void NavigateToLogin()
        {
            
            App.Provider!.GetRequiredService<LoginWindow>().Show();
            CloseCurrent();
        }

        public void NavigationToForgotPassword()
        {
            
            App.Provider!.GetRequiredService<ForgotPasswordWindow>().Show();
            CloseCurrent();
        }
    }
}
