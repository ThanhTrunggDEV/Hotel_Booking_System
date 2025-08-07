using Hotel_Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.ViewModels
{
    internal class LoginViewModel : ILoginViewModel
    {
        private readonly INavigationService _navigationService;
        public string Password { get ; set ; }

        public LoginViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Login(string username)
        {

            _navigationService.NavigateToUserWindow();
            _navigationService.NavigateToAdminWindow();
        }
    }
}
