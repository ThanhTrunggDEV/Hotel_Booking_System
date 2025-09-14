using System;
using System.Windows;
using System.Windows.Threading;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    public partial class ModifyBookingDialog : Window
    {
        private readonly IRoomRepository _roomRepository = App.Provider!.GetRequiredService<IRoomRepository>();
        private DispatcherTimer? _notificationTimer;

        public ModifyBookingDialog()
        {
            InitializeComponent();
            btnSave.Click += BtnSave_Click;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            txtNotification.Visibility = Visibility.Collapsed;
            txtNotification.Text = string.Empty;

            if (DataContext is not Booking booking)
            {
                DialogResult = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(booking.GuestName))
            {
                ShowNotification("Customer name is required.");
                return;
            }

            if (booking.NumberOfGuests <= 0)
            {
                ShowNotification("Number of guests must be at least 1.");
                return;
            }

            var room = await _roomRepository.GetByIdAsync(booking.RoomID);
            if (room != null && booking.NumberOfGuests > room.Capacity)
            {
                ShowNotification("Number of guests exceeds room capacity.");
                return;
            }

            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ShowNotification("Check-out date must be later than check-in date.");
                return;
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                ShowNotification("Check-in date cannot be in the past.");
                return;
            }

            DialogResult = true;
        }

        private void ShowNotification(string message)
        {
            txtNotification.Text = message;
            txtNotification.Visibility = Visibility.Visible;

            _notificationTimer?.Stop();
            _notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _notificationTimer.Tick += (s, e) =>
            {
                txtNotification.Visibility = Visibility.Collapsed;
                txtNotification.Text = string.Empty;
                _notificationTimer.Stop();
            };
            _notificationTimer.Start();
        }
    }
}
