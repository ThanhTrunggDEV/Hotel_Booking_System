using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Hotel_Booking_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        IAdminViewModel _viewModel = App.Provider!.GetRequiredService<IAdminViewModel>();
        public SuperAdminWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
