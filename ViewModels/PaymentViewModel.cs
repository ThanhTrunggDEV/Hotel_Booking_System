using CommunityToolkit.Mvvm.Input;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Manager.FrameWorks;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Hotel_Booking_System.ViewModels
{
    internal partial class PaymentViewModel : Bindable, IPaymentViewModel
    {
        private readonly IPaymentRepository _paymentRepository;
        private string _bookingId = string.Empty;
        private double _totalPayment;
        private string _method = "Cash";
        public ObservableCollection<Payment> Payments { get; } = new();

        public PaymentViewModel(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public string BookingID
        {
            get => _bookingId;
            set => Set(ref _bookingId, value);
        }

        public double TotalPayment
        {
            get => _totalPayment;
            set => Set(ref _totalPayment, value);
        }

        public string Method
        {
            get => _method;
            set => Set(ref _method, value);
        }

        public double TotalRevenue => Payments.Sum(p => p.TotalPayment);

        public async Task LoadPaymentsAsync()
        {
            Payments.Clear();
            var payments = await _paymentRepository.GetAllAsync();
            foreach (var p in payments)
                Payments.Add(p);
            PropertyChanged?.Invoke(this, new(nameof(TotalRevenue)));
        }

        [RelayCommand]
        private async Task ConfirmPayment()
        {
            var payment = new Payment
            {
                BookingID = BookingID,
                TotalPayment = TotalPayment,
                Method = Method,
                PaymentDate = DateTime.Now
            };
            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveAsync();
            Payments.Add(payment);
            PropertyChanged?.Invoke(this, new(nameof(TotalRevenue)));
            MessageBox.Show("Payment Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
