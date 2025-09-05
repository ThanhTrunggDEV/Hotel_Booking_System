using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Services
{
    public class MessageService : ValueChangedMessage<string>
    {   
        public MessageService(string value) : base(value) { }
    }
}
