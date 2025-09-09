using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using DotNetEnv;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Repository;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.ViewModels;
using Hotel_Booking_System.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Booking_System
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Env.Load();
        }
        private static IServiceProvider? provider;
        public static IServiceProvider? Provider { get => provider ??= ConfigDI(); }
        private static IServiceProvider ConfigDI()
        {
            return new ServiceCollection().AddDbContext<AppDbContext>()

                               .AddSingleton<LoginWindow>()
                               .AddSingleton<ForgotPasswordWindow>()
                               .AddSingleton<SignUpWindow>()
                               .AddSingleton<UserWindow>()
                               .AddSingleton<BookingDialog>()
                               .AddSingleton<AdminWindow>()

                               .AddScoped<IHotelRepository, HotelRepository>()
                               .AddScoped<IBookingRepository, BookingRepository>()
                               .AddScoped<IUserRepository, UserRepository>()
                                 .AddScoped<IReviewRepository, ReviewRepository>()
                                 .AddScoped<IPaymentRepository, PaymentRepository>()
                                 .AddScoped<IAmenityRepository, AmenityRepository>()
                                 .AddScoped<IAIChatRepository, AIChatRepository>()
                               .AddScoped<IRoomRepository, RoomRepository>()
                               .AddScoped<IHotelAdminRequestRepository, HotelAdminRequestRepository>()

                               .AddScoped<INavigationService, NavigationService>()
                               .AddScoped<IAuthentication, AuthenticationSerivce>()

                              .AddScoped<ILoginViewModel, LoginViewModel>()
                              .AddScoped<IBookingViewModel, BookingViewModel>()
                              .AddScoped<IForgotPasswordViewModel, ForgotPasswordViewModel>()
                              .AddScoped<ISignUpViewModel, SignUpViewModel>()
                              .AddScoped<IUserViewModel, UserViewModel>()
                              .BuildServiceProvider();
        }
    }

}
