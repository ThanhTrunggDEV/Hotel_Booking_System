using System;
using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Net.Http;
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
            var geminiOptions = new GeminiOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty,
                DefaultModel = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash"
            };

            return new ServiceCollection().AddDbContext<AppDbContext>()

                               .AddTransient<LoginWindow>()
                               .AddTransient<ForgotPasswordWindow>()
                               .AddTransient<SignUpWindow>()
                               .AddTransient<UserWindow>()
                              .AddTransient<BookingDialog>()
                              .AddTransient<AdminWindow>()
                               .AddTransient<BookingDialog>()
                               .AddTransient<PaymentDialog>()
                               .AddTransient<AdminWindow>()
                               .AddTransient<SuperAdminWindow>()

                               .AddScoped<IHotelRepository, HotelRepository>()
                               .AddSingleton<IBookingRepository, BookingRepository>()
                               .AddScoped<IUserRepository, UserRepository>()
                                 .AddScoped<IReviewRepository, ReviewRepository>()
                                 .AddScoped<IPaymentRepository, PaymentRepository>()
                                 .AddScoped<IAmenityRepository, AmenityRepository>()
                                .AddScoped<IAIChatRepository, AIChatRepository>()
                                  .AddScoped<IAmenityRepository, AmenityRepository>()
                                  .AddSingleton(geminiOptions)
                                  .AddScoped<IAIChatRepository, AIChatRepository>()
                                  .AddScoped<IAIChatService, AIChatService>()
                        
                               .AddScoped<IAIChatRepository, AIChatRepository>()
                                 .AddScoped<IAIChatService, AIChatService>()
                                 .AddSingleton(new HttpClient())
                                 .AddSingleton(provider => new GeminiOptions
                                 {
                                     ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty,
                                     DefaultModel = Environment.GetEnvironmentVariable("GEMINI_DEFAULT_MODEL") ?? string.Empty
                                 })

                               .AddScoped<IRoomRepository, RoomRepository>()
                                .AddScoped<IHotelAdminRequestRepository, HotelAdminRequestRepository>()

                               .AddScoped<INavigationService, NavigationService>()
                               .AddScoped<IAuthentication, AuthenticationSerivce>()

                              .AddScoped<ILoginViewModel, LoginViewModel>()
                              .AddScoped<IBookingViewModel, BookingViewModel>()
                              .AddScoped<IPaymentViewModel, PaymentViewModel>()
                              .AddScoped<IForgotPasswordViewModel, ForgotPasswordViewModel>()
                              .AddScoped<ISignUpViewModel, SignUpViewModel>()
                              .AddScoped<IAdminViewModel, AdminViewModel>()
                              .AddScoped<IUserViewModel, UserViewModel>()
                              .AddScoped<IAdminViewModel, AdminViewModel>()
                              .BuildServiceProvider();
        }
    }

}
