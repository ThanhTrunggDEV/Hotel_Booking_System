using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    internal interface IBookingViewModel
    {
        Room? SelectedRoom { get; set; }
        User CurrentUser { get; set; }
        Hotel Hotel{ get; set; }
        string GuestName { get; set; }
    }
}
