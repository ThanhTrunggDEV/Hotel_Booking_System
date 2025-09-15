using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Hotel_Booking_System.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<AIChat> AIChats { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<HotelAdminRequest> HotelAdminRequests { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Amenity> Amenities { get; set; }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=data.dat");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.UserID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Room>()
                .Property(u => u.RoomID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Hotel>()
                .HasMany(h => h.Amenities)
                .WithMany(a => a.Hotels)
                .UsingEntity(j => j.ToTable("HotelAmenity"))
                .Property(u => u.HotelID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Booking>()
                .Property(u => u.BookingID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Review>()
                .Property(u => u.ReviewID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Payment>()
                .Property(u => u.PaymentID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<AIChat>()
                .Property(u => u.ChatID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HotelAdminRequest>()
                .Property(u => u.RequestID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Amenity>()
                .Property(u => u.AmenityID)
                .HasValueGenerator<StringGuidValueGenerator>()
                .ValueGeneratedOnAdd();

        }

        public void SeedData()
        {
            Database.EnsureCreated();

            var authentication = new AuthenticationSerivce();

            if (!Users.Any())
            {
                var superAdmin = new User
                {
                    FullName = "Super Admin",
                    Email = "superadmin@gmail.com",
                    Password = authentication.HashPassword("123456"),
                    Role = "SuperAdmin",
                    DateOfBirth = DateTime.Now
                };

                var hotelAdmin = new User
                {
                    FullName = "Hotel Admin",
                    Email = "hoteladmin@gmail.com",
                    Password = authentication.HashPassword("123456"),
                    Role = "HotelAdmin",
                    DateOfBirth = DateTime.Now
                };

                var customer = new User
                {
                    FullName = "John Doe",
                    Email = "customer@gmail.com",
                    Password = authentication.HashPassword("123456"),
                    Role = "User",
                    DateOfBirth = DateTime.Now
                };

                Users.AddRange(superAdmin, hotelAdmin, customer);
                SaveChanges();
            }

            var admin = Users.First(u => u.Email == "hoteladmin@gmail.com");
            var customerUser = Users.First(u => u.Email == "customer@gmail.com");

            if (!Amenities.Any())
            {
                var wifi = new Amenity { AmenityName = "Free WiFi" };
                var pool = new Amenity { AmenityName = "Pool" };
                Amenities.AddRange(wifi, pool);
                SaveChanges();
            }

            if (!Hotels.Any())
            {
                var hotels = new List<Hotel>();
                for (int i = 1; i <= 5; i++)
                {
                    hotels.Add(new Hotel
                    {
                        UserID = admin.UserID,
                        HotelName = $"Sample Hotel {i}",
                        Address = $"{i} Sample Street",
                        City = "Sample City",
                        HotelImage = "https://example.com/hotel.jpg",
                        Description = "A cozy sample hotel",
                        Rating = 4,
                        IsApproved = true,
                        IsVisible = true,
                        Amenities = Amenities.ToList()
                    });
                }
                Hotels.AddRange(hotels);
                SaveChanges();
            }

            var hotelsList = Hotels.ToList();

            if (!Rooms.Any())
            {
                var random = new Random();
                foreach (var hotel in hotelsList)
                {
                    int roomCount = random.Next(3, 11);
                    var prices = new List<double>();
                    for (int i = 1; i <= roomCount; i++)
                    {
                        double price = random.Next(50, 301);
                        prices.Add(price);
                        Rooms.Add(new Room
                        {
                            HotelID = hotel.HotelID,
                            RoomNumber = $"{100 + i}",
                            RoomImage = "https://example.com/room.jpg",
                            RoomType = "Standard",
                            Capacity = random.Next(1, 5),
                            PricePerNight = price,
                            Status = "Available"
                        });
                    }
                    hotel.MinPrice = prices.Min();
                    hotel.MaxPrice = prices.Max();
                }
                SaveChanges();
            }

            var hotelEntity = hotelsList.First();
            var roomEntity = Rooms.First(r => r.HotelID == hotelEntity.HotelID);

            if (!Bookings.Any())
            {
                var booking = new Booking
                {
                    HotelID = hotelEntity.HotelID,
                    RoomID = roomEntity.RoomID,
                    UserID = customerUser.UserID,
                    GuestName = customerUser.FullName,
                    NumberOfGuests = 1,
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(2),
                    Status = "Confirmed"
                };
                Bookings.Add(booking);
                SaveChanges();
            }

            var bookingEntity = Bookings.First();

            if (!Payments.Any())
            {
                var payment = new Payment
                {
                    BookingID = bookingEntity.BookingID,
                    TotalPayment = 200,
                    Method = "CreditCard",
                    PaymentDate = DateTime.Today
                };
                Payments.Add(payment);
                SaveChanges();
            }

            if (!Reviews.Any())
            {
                var random = new Random();
                var reviews = new List<Review>();
                for (int i = 1; i <= 25; i++)
                {
                    reviews.Add(new Review
                    {
                        UserID = customerUser.UserID,
                        HotelID = hotelEntity.HotelID,
                        RoomID = roomEntity.RoomID,
                        BookingID = bookingEntity.BookingID,
                        Rating = random.Next(1, 6),
                        Comment = $"Sample review {i}",
                        CreatedAt = DateTime.Now.AddDays(-i)
                    });
                }
                Reviews.AddRange(reviews);
                SaveChanges();
            }

            if (!AIChats.Any())
            {
                var chat = new AIChat
                {
                    UserID = customerUser.UserID,
                    Message = "Hello",
                    Response = "Welcome to our service",
                    CreatedAt = DateTime.Now
                };
                AIChats.Add(chat);
                SaveChanges();
            }

            if (!HotelAdminRequests.Any())
            {
                HotelAdminRequests.Add(new HotelAdminRequest
                {
                    UserID = admin.UserID,
                    HotelName = "Sample Hotel",
                    HotelAddress = "123 Sample Street",
                    Reason = "Initial request",
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                });
                SaveChanges();
            }
        }


    }
}
