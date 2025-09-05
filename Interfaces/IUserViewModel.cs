using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Interfaces
{
    interface IUserViewModel
    {
        User CurrentUser { get; set; }
        Hotel CurrentHotel { get; set; }
        string ShowAvailableHotels { get; set; }
        string ShowRoomList  { get; set; }

    }
}
