using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

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
    public interface IOpenBookingDialog
    {
        void OpenBookingDialog(Room room, User currentUser, string hotelName);
    }
    public interface INavigationService : INavigationToAdmin, INavigationToSignUp, INavigationToUser, INavitionToLogin, NavigationToForgotPassword, IOpenBookingDialog
    {
        
    }
}
