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

        public void NavigateToSuperAdmin()
        {



            var newWindow = App.Provider!.GetRequiredService<SuperAdminWindow>();
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

        public bool OpenBookingDialog(Room room, User currentUser, Hotel hotel)
        {
            var bookingWindow = App.Provider!.GetRequiredService<BookingDialog>();

            bookingWindow.btnCancel.Click += (s, e) =>
            {
                bookingWindow.DialogResult = false;
                bookingWindow.Close();
            };

            var vm = App.Provider!.GetRequiredService<IBookingViewModel>();
            vm.SelectedRoom = room;
            vm.CurrentUser = currentUser;
            vm.Hotel = hotel;
            vm.GuestName = currentUser.FullName;
            
            bookingWindow.DataContext = vm;
            bookingWindow.ShowDialog();
            return bookingWindow.DialogResult == true;
            
        }
        public void CloseBookingDialog()
        {
            var bookingWindow = Application.Current.Windows.OfType<BookingDialog>().SingleOrDefault();
            if (bookingWindow != null)
            {
                bookingWindow.DialogResult = true;
                bookingWindow.Close();
            }
        }
        public bool OpenReviewDialog(Booking booking)
        {
            var reviewWindow = App.Provider!.GetRequiredService<ReviewDialog>();

            reviewWindow.btnCancel.Click += (s, e) =>
            {
                reviewWindow.DialogResult = false;
                reviewWindow.Close();
            };
            reviewWindow.btnSubmit.Click += (s, e) => reviewWindow.DialogResult = true;

            var vm = App.Provider!.GetRequiredService<IReviewViewModel>();
            vm.Booking = booking;

            reviewWindow.DataContext = vm;
            reviewWindow.ShowDialog();
            return reviewWindow.DialogResult == true;
        }


        public bool OpenPaymentDialog(string bookingId, double amount)
        {
            var paymentWindow = App.Provider!.GetRequiredService<PaymentDialog>();

            paymentWindow.btnCancel.Click += (s, e) =>
            {
                paymentWindow.DialogResult = false;
                paymentWindow.Close();
            };
            paymentWindow.btnPay.Click += (s, e) => paymentWindow.DialogResult = true;

            var vm = App.Provider!.GetRequiredService<IPaymentViewModel>();
            vm.BookingID = bookingId;
            vm.TotalPayment = amount;
            paymentWindow.DataContext = vm;
            paymentWindow.ShowDialog();
            return paymentWindow.DialogResult == true;
        }

        public bool OpenModifyDialog(Booking booking)
        {
            var modifyWindow = App.Provider!.GetRequiredService<ModifyBookingDialog>();

            modifyWindow.btnCancel.Click += (s, e) =>
            {
                modifyWindow.DialogResult = false;
                modifyWindow.Close();
            };

            modifyWindow.DataContext = booking;
            modifyWindow.ShowDialog();
            return modifyWindow.DialogResult == true;
        }
        public void ClosePaymentDialog()
        {
            CloseCurrent();
        }

        public void NavigateToHotel()
        {
            var newWindow = App.Provider!.GetRequiredService<HotelAdminWindow>();


            Application.Current.MainWindow = newWindow;

            CloseCurrent();
            newWindow.Show();
        }
    }
}
