using System.Configuration;
using System.Data;
using System.Windows;
using Hotel_Manager.Interfaces;
using Hotel_Manager.Repository;
using Hotel_Manager.Services;
using Hotel_Manager.ViewModels;
using Hotel_Manager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IServiceProvider? provider;
        public static IServiceProvider? Provider { get => provider ??= ConfigDI(); }
        private static IServiceProvider ConfigDI()
        {
            return new ServiceCollection().AddDbContext<AppDbContext>()
                               .AddScoped<LoginWindow>()
                               .AddScoped<UserWindow>()
                               .AddScoped<AdminWindow>()
                               .AddScoped<IUserRepository, UserRepository>()
                               .AddScoped<IRoomRepository, RoomRepository>()
                               .AddScoped<INavigationService, NavigationService>()
                               .AddScoped<IAuthentication, AuthenticationSerivce>()
                              .AddScoped<ILoginViewModel, LoginViewModel>()
                              .BuildServiceProvider();
        }
        
    }

}
