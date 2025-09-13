using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hotel_Booking_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        public HotelAdminViewModel ViewModel { get; }

        public HotelAdminWindow()
        {
            InitializeComponent();
            ViewModel = App.Provider.GetRequiredService<HotelAdminViewModel>();
            DataContext = ViewModel;
        }
    }
}
