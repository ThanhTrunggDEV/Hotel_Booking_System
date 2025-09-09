using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.ViewModels
{
    internal partial class BookingViewModel : Bindable, IBookingViewModel
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly INavigationService _navigationService;

        private DateTime _checkInDate = DateTime.Now;
        private DateTime _checkOutDate = DateTime.Now;
        private int _numberOfGuests;
        private double _totalPayment;

        public BookingViewModel(IBookingRepository bookingRepository, IRoomRepository roomRepository, INavigationService navigationService)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _navigationService = navigationService;
        }

        public double TotalPayment
        {
            get => _totalPayment;
            set => Set(ref _totalPayment, value);
        }

        public int NumberOfGuests
        {
            get => _numberOfGuests;
            set => Set(ref _numberOfGuests, value);
        }
        public DateTime CheckInDate
        {
            get => _checkInDate;
            set
            {
                Set(ref _checkInDate, value);
                CalculateTotalPayment();
            }
        }
        public DateTime CheckOutDate
        {
            get => _checkOutDate;
            set
            {
                Set(ref _checkOutDate, value);
                CalculateTotalPayment();
            }
        }
        public Room SelectedRoom { get ; set ; }
        public User CurrentUser { get ; set ; }
        public Hotel Hotel { get ; set ; }

        private void CalculateTotalPayment()
        {
            if (SelectedRoom == null)
            {
                TotalPayment = 0;
                return;
            }
            var days = (CheckOutDate - CheckInDate).Days;
            if (days <= 0)
            {
                TotalPayment = 0;
                return;
            }
            TotalPayment = days * SelectedRoom.PricePerNight;
        }


        [RelayCommand]
        private async Task ConfirmBooking()
        {
            if (SelectedRoom == null || CurrentUser == null)
                return;
            var booking = new Booking
            {
                UserID = CurrentUser.UserID,
                RoomID = SelectedRoom.RoomID,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                Status = "Pending",
                HotelID = Hotel.HotelID


            };
            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveAsync();
            
            MessageBox.Show("Booked Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            _navigationService.CloseBookingDialog();


        }
    }
}
