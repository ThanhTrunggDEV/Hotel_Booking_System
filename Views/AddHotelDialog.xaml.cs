using System.Windows;

namespace Hotel_Booking_System.Views
{
    public partial class AddHotelDialog : Window
    {
        public AddHotelDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
