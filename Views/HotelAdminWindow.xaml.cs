using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        private readonly IHotelAdminViewModel _viewModel;

        public HotelAdminWindow()
        {
            InitializeComponent();
            _viewModel = App.Provider.GetRequiredService<IHotelAdminViewModel>();
            DataContext = _viewModel;
         private readonly IHotelAdminViewModel  _hotelAdminViewModel = App.Provider.GetRequiredService<IHotelAdminViewModel>();
        public HotelAdminWindow()
        {
            InitializeComponent();
            DataContext = _hotelAdminViewModel;
            Loaded += async (s, e) => await _hotelAdminViewModel.LoadReviewsAsync();
        }
    }
}
