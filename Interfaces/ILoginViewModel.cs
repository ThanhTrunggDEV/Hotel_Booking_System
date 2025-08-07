using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Interfaces
{
    interface ILoginViewModel
    {
        public string Password { get; set; }
        void Login(string username);
    }
}
