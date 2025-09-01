using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    class AIChat
    {
        public string ChatID {  get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
