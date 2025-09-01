using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hotel_Booking_System.Services;
using Hotel_Booking_System.DomainModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.ViewModels
{
    public partial class UserViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly INavigationService _navigationService;
        
        private string userMail;

        public ObservableCollection<Hotel> Hotels
        {
            get;
            set;
        } = new ObservableCollection<Hotel>();

        public UserViewModel()
        {

            WeakReferenceMessenger.Default.Register<MessageService>(this, (r, msg) =>
            {
                userMail = msg.Value;
            });

            LoadHotels();

        }
        private void LoadHotels()
        {
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description hgfhgfhdfghgfd dgf gfhgjhfjkhgfdhgdfhgfhdfhdgf hgdfh fgdhnjkgfdh jkfgdhkjgdfh dfghdfg fgdhfgd ghfgdhfdghgdfhdfg1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
            Hotels.Add(new Hotel { HotelName = "Hotel 1", Address = "Address 1", City = "City 1", Description = "Description 1", Phone = "1234567890", Rating = 4, HotelImage = "https://via.placeholder.com/150" });
        }

    }
}
