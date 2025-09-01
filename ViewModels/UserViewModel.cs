using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.DomainModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Hotel_Booking_System.ViewModels
{
    public partial class UserViewModel
    {
        private string userMail;

        public UserViewModel()
        {

            WeakReferenceMessenger.Default.Register<MessageService>(this, (r, msg) =>
            {
                userMail = msg.Value;
            });
        }

    }
}
