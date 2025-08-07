using Hotel_Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Services
{
    class NavigationService : INavigationService
    {
        private void CloseCurrent()
        {
            App.Current.Windows[0].Close();
        }
        public void NavigateToAdminWindow()
        {
            CloseCurrent();
        }

        public void NavigateToUserWindow()
        {
            CloseCurrent();
        }
    }
}
