using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Services
{
    public class BookRoomMessage : ValueChangedMessage<(Room Room, User User)>
    {
        public BookRoomMessage(Room room, User user) : base((room, user)) { }
    }
}
