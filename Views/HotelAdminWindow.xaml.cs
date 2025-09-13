using System.Windows;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        public HotelAdminWindow()
        {
            InitializeComponent();
            DataContext = App.Provider.GetRequiredService<IHotelAdminViewModel>();
        }
    }
}
