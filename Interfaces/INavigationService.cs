using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    public interface INavigationToSignUp
    {
        void NavigateToSignUp();
    }
    public interface INavigationToUser
    {
        void NavigateToUser();
    }
    public interface INavigationToAdmin
    {
        void NavigateToAdmin();
    }
    public interface INavitionToLogin
    {
        void NavigateToLogin();
    }
    public interface NavigationToForgotPassword
    {
        void NavigationToForgotPassword();
    }
    public interface INavigationService : INavigationToAdmin, INavigationToSignUp, INavigationToUser, INavitionToLogin, NavigationToForgotPassword
    {
        
    }
}
