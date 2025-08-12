using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    interface INavigationToSignUp
    {
        void NavigateToSignUp();
    }
    interface INavigationToUser
    {
        void NavigateToUser();
    }
    interface INavigationToAdmin
    {
        void NavigateToAdmin();
    }
    interface INavitionToLogin
    {
        void NavigateToLogin();
    }
    interface NavigationToForgotPassword
    {
        void NavigationToForgotPassword();
    }
    interface INavigationService : INavigationToAdmin, INavigationToSignUp, INavigationToUser, INavitionToLogin, NavigationToForgotPassword
    {
        
    }
}
