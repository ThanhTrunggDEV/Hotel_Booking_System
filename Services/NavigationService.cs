using Hotel_Manager.Interfaces;
using Hotel_Manager.Views;
using Microsoft.Extensions.DependencyInjection;
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
            App.Provider.GetRequiredService<AdminWindow>().Show();
        }

        public void NavigateToUserWindow()
        {
            CloseCurrent();
            App.Provider.GetRequiredService<UserWindow>().Show();
        }
    }
}
