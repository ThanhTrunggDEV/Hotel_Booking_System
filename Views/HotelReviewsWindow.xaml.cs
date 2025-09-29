using System.Windows;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelReviewsWindow.xaml
    /// </summary>
    public partial class HotelReviewsWindow : Window
    {
        public HotelReviewsWindow()
        {
            InitializeComponent();
            CloseButton.Click += (_, __) => Close();
            CloseFooterButton.Click += (_, __) => Close();
        }
    }
}
