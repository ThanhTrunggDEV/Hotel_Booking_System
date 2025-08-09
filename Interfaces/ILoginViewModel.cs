using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hotel_Manager.Interfaces
{
    interface ILoginViewModel
    {
        public string Password { get; set; }
        IRelayCommand LoginCommand { get; set; }
    }
}
