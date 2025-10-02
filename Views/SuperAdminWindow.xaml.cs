using System.ComponentModel;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;


namespace Hotel_Booking_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        private readonly ISuperAdminViewModel _superAdminViewModel = App.Provider.GetRequiredService<ISuperAdminViewModel>();

        public SuperAdminWindow()
        {
            InitializeComponent();
            DataContext = _superAdminViewModel;

            txtCurrentPassword.PasswordChanged += (s, e) =>
            {
                (_superAdminViewModel as dynamic).CurrentPassword = txtCurrentPassword.Password;
            };

            txtNewPassword.PasswordChanged += (s, e) =>
            {
                (_superAdminViewModel as dynamic).NewPassword = txtNewPassword.Password;
            };

            txtConfirmPassword.PasswordChanged += (s, e) =>
            {
                (_superAdminViewModel as dynamic).ConfirmPassword = txtConfirmPassword.Password;
            };

            if (_superAdminViewModel is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += SuperAdminViewModel_PropertyChanged;
            }

            Loaded += async (s, e) =>
            {
                await _superAdminViewModel.LoadDataAsync();
            };
        }

        private void SuperAdminViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_superAdminViewModel is not SuperAdminViewModel vm)
            {
                return;
            }

            if (e.PropertyName == nameof(SuperAdminViewModel.CurrentPassword) && string.IsNullOrEmpty(vm.CurrentPassword))
            {
                txtCurrentPassword.Password = string.Empty;
            }

            if (e.PropertyName == nameof(SuperAdminViewModel.NewPassword) && string.IsNullOrEmpty(vm.NewPassword))
            {
                txtNewPassword.Password = string.Empty;
            }

            if (e.PropertyName == nameof(SuperAdminViewModel.ConfirmPassword) && string.IsNullOrEmpty(vm.ConfirmPassword))
            {
                txtConfirmPassword.Password = string.Empty;
            }
        }

        private void CancelPassword_Click(object sender, RoutedEventArgs e)
        {
            txtCurrentPassword.Password = string.Empty;
            txtNewPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;

            var command = (_superAdminViewModel as dynamic).ClearPasswordFieldsCommand;
            if (command is ICommand clearCommand && clearCommand.CanExecute(null))
            {
                clearCommand.Execute(null);
            }
        }


    }
}
