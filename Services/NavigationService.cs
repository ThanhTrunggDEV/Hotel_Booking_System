using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Services
{
    class NavigationService : INavigationService
    {
        private void CloseCurrent()
        {
            App.Current.Windows[0].Close();
        }
        public void NavigateToAdmin()
        {
            CloseCurrent();
            App.Provider!.GetRequiredService<AdminWindow>().Show();
        }

        public void NavigateToUser()
        {
            CloseCurrent();
            App.Provider!.GetRequiredService<UserWindow>().Show();
        }

     
        public void NavigateToSignUp()
        {
            CloseCurrent();
            App.Provider!.GetRequiredService<SignUpWindow>().Show();
        }

        public void NavigateToLogin()
        {
            CloseCurrent();
            App.Provider!.GetRequiredService<LoginWindow>().Show();
        }

        public void NavigationToForgotPassword()
        {
            CloseCurrent();
            App.Provider!.GetRequiredService<ForgotPasswordWindow>().Show();
        }
    }
}
