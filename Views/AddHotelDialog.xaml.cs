using System.Windows;
using Microsoft.Win32;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Services;

namespace Hotel_Booking_System.Views
{
    public partial class AddHotelDialog : Window
    {
        public AddHotelDialog()
        {
            InitializeComponent();
        }

        private async void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            FileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true && DataContext is Hotel hotel)
            {
                var uploadedPath = await UploadImageService.UploadAsync(openFileDialog.FileName);
                hotel.HotelImage = uploadedPath;
            }
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
