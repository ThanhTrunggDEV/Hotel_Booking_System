using Hotel_Booking_System.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hotel_Booking_System.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            DataContext = new UserViewModel();

           
            btnChat.Click += BtnChat_Click;
            btnCloseChat.Click += BtnCloseChat_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void BtnChat_Click(object sender, RoutedEventArgs e)
        {
            
            AIChatPanel.Visibility = Visibility.Visible;
            btnChat.Visibility = Visibility.Collapsed;
        }

        private void BtnCloseChat_Click(object sender, RoutedEventArgs e)
        {
           
            AIChatPanel.Visibility = Visibility.Collapsed;
            btnChat.Visibility = Visibility.Visible;
        }
    }
}