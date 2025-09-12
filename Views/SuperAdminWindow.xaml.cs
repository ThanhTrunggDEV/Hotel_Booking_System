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
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        private readonly IPaymentViewModel _paymentViewModel = App.Provider.GetRequiredService<IPaymentViewModel>();
        private readonly IAdminViewModel _adminViewModel = App.Provider.GetRequiredService<IAdminViewModel>();
        public SuperAdminWindow()
        {
            InitializeComponent();
            PaymentSummaryTab.DataContext = _paymentViewModel;
            Loaded += async (s, e) => await _paymentViewModel.LoadPaymentsAsync();
            DataContext = _adminViewModel;
        }
       
     
    }
}
