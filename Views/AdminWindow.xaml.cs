using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Hotel_Booking_System.Views
{
    public partial class AdminWindow : Window
    {
        IAdminViewModel _viewModel = App.Provider!.GetRequiredService<IAdminViewModel>();
        public AdminWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
