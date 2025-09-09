using Hotel_Booking_System.DomainModels;
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
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)!;
            currentWindow.Close();

        }

        public void NavigateToAdmin()
        {

            var newWindow = App.Provider!.GetRequiredService<AdminWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();
        }

        public void NavigateToUser()
        {
            var newWindow = App.Provider!.GetRequiredService<UserWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();
        }

     
        public void NavigateToSignUp()
        {

            var newWindow = App.Provider!.GetRequiredService<SignUpWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();

        }

        public void NavigateToLogin()
        {

            var newWindow = App.Provider!.GetRequiredService<LoginWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();
        }

        public void NavigationToForgotPassword()
        {

            var newWindow = App.Provider!.GetRequiredService<ForgotPasswordWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();
        }

        public void OpenBookingDialog(Room room, User currentUser, string hotelName)
        {
            var bookingWindow = App.Provider!.GetRequiredService<BookingDialog>();
            var vm = App.Provider!.GetRequiredService<IBookingViewModel>();
            vm.SelectedRoom = room;
            vm.CurrentUser = currentUser;
            vm.HotelName = hotelName;
            bookingWindow.DataContext = vm;
            bookingWindow.ShowDialog();
            
        }
    }
}
