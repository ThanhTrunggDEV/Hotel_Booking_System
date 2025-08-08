using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.DomainModels
{
    internal class Room
    {
        public string RoomID { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string RoomTypeID { get; set; } = "";
        public string Position { get; set; } = "";
        public bool IsAvailable { get; set; }
    }
}
