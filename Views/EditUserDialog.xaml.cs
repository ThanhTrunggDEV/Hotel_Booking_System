using System;
using System.Windows;

namespace Hotel_Booking_System.Views
{
    public partial class EditUserDialog : Window
    {
        public bool IsDeleteRequested { get; private set; }

        public EditUserDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IsDeleteRequested = false;

            if (DataContext is not DomainModels.User user)
            {
                DialogResult = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                MessageBox.Show("Full name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (user.DateOfBirth == default)
            {
                MessageBox.Show("Please select a valid date of birth.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                MessageBox.Show("Please select a user role.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not DomainModels.User user)
            {
                MessageBox.Show("No user selected for deletion.", "Delete user", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản {user.FullName}?", "Xác nhận xóa",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            IsDeleteRequested = true;
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
