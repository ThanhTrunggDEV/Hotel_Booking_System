using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for HotelAdminWindow.xaml
    /// </summary>
    public partial class HotelAdminWindow : Window
    {
        private readonly IReviewRepository _reviewRepository;
        public ObservableCollection<Review> Reviews { get; set; }

        public HotelAdminWindow()
        {
            InitializeComponent();
            _reviewRepository = App.Provider.GetRequiredService<IReviewRepository>();
            Reviews = new ObservableCollection<Review>(_reviewRepository.GetAllAsync().Result);
            DataContext = this;
        }
    }
}
