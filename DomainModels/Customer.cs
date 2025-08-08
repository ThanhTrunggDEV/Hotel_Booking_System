using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.DomainModels
{
    internal class Customer
    {
        public string CustomerID { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string IDNumber { get; set; } = "";
    }
}
