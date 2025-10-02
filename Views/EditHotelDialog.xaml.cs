using System;
using System.Windows;

namespace Hotel_Booking_System.Views
{
    public partial class EditHotelDialog : Window
    {
        public EditHotelDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not DomainModels.Hotel hotel)
            {
                DialogResult = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(hotel.HotelName) || string.IsNullOrWhiteSpace(hotel.City) || string.IsNullOrWhiteSpace(hotel.Address))
            {
                MessageBox.Show("Hotel name, city and address are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (hotel.MinPrice < 0 || hotel.MaxPrice < 0)
            {
                MessageBox.Show("Prices cannot be negative.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (hotel.MaxPrice < hotel.MinPrice)
            {
                MessageBox.Show("Maximum price cannot be lower than minimum price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (hotel.Rating < 0 || hotel.Rating > 5)
            {
                MessageBox.Show("Rating must be between 0 and 5.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
