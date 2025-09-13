using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Hotel_Booking_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        private readonly IPaymentViewModel _paymentViewModel = App.Provider.GetRequiredService<IPaymentViewModel>();
        private readonly IAdminViewModel _adminViewModel = App.Provider.GetRequiredService<IAdminViewModel>();
        private readonly ISuperAdminViewModel _superAdminViewModel = App.Provider.GetRequiredService<ISuperAdminViewModel>();

        public SuperAdminWindow()
        {
            InitializeComponent();
            DataContext = _superAdminViewModel;
            PaymentSummaryTab.DataContext = _paymentViewModel;
            AdminBookingsRequestsTab.DataContext = _adminViewModel;
            Loaded += async (s, e) =>
            {
                await _paymentViewModel.LoadPaymentsAsync();
                await _superAdminViewModel.LoadDataAsync();
            };
        }
       
     
    }
}
