using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
   partial class UserViewModel
    {
        private string userMail;
        public UserViewModel()
        {
            WeakReferenceMessenger.Default.Register<MessageService>(this, (r, msg) =>
            {
                userMail = msg.Value;
            });
        }

      
        [RelayCommand]
        private void Test()
        {
            MessageBox.Show(userMail);
        }
    }
}
