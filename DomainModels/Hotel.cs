using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.DomainModels
{
    internal class Hotel
    {
        public string HotelID { get; set; } = "";
        public string HotelName { get; set; } = "";
        public string HotelAddress { get; set; } = "";
        public string HotelImage { get; set; } = "";
        public string HotelDescription { get; set; } = "";
    }
}
