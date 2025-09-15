using System.Windows;
using Microsoft.Win32;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.DomainModels;

namespace Hotel_Booking_System.Views
{
    public partial class EditRoomDialog : Window
    {
        public EditRoomDialog()
        {
            InitializeComponent();
        }

        private async void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            FileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true && DataContext is Room room)
            {
                room.RoomImage = openFileDialog.FileName;
                var uploadedPath = await UploadImageService.UploadAsync(openFileDialog.FileName);
                room.RoomImage = uploadedPath;
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
