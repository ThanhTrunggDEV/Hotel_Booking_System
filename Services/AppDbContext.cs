using System;
using System.Collections.Generic;
using System.Data;
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
            EnsureReviewReplyColumn();

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
            var hotelRooms = Rooms.Where(r => r.HotelID == hotelEntity.HotelID).ToList();
            if (hotelRooms.Count == 0)
            {
                hotelRooms.Add(roomEntity);
            }

            var paymentSeedData = new List<(Booking booking, double total, DateTime paymentDate, string method)>();

            if (!Bookings.Any())
            {
                var random = new Random();
                var referenceMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var newBookings = new List<Booking>();

                for (int i = 0; i < 18; i++)
                {
                    var monthStart = referenceMonth.AddMonths(-i);
                    var room = hotelRooms[random.Next(hotelRooms.Count)];
                    var nights = random.Next(2, 6);
                    var latestStart = Math.Max(1, DateTime.DaysInMonth(monthStart.Year, monthStart.Month) - nights - 1);
                    var checkIn = monthStart.AddDays(random.Next(0, latestStart));
                    var checkOut = checkIn.AddDays(nights);
                    var guests = Math.Min(room.Capacity, random.Next(1, room.Capacity + 1));

                    var booking = new Booking
                    {
                        HotelID = hotelEntity.HotelID,
                        RoomID = room.RoomID,
                        UserID = customerUser.UserID,
                        GuestName = customerUser.FullName,
                        NumberOfGuests = guests == 0 ? 1 : guests,
                        CheckInDate = checkIn,
                        CheckOutDate = checkOut,
                        Status = "Completed"
                    };

                    newBookings.Add(booking);

                    var method = random.Next(0, 3) switch
                    {
                        0 => "CreditCard",
                        1 => "BankTransfer",
                        _ => "Cash"
                    };
                    var occupancyFactor = 0.85 + (random.NextDouble() * 0.35);
                    var total = room.PricePerNight * nights * occupancyFactor;
                    var paymentDate = checkOut.AddDays(random.Next(0, 3));

                    paymentSeedData.Add((booking, total, paymentDate, method));
                }

                Bookings.AddRange(newBookings);
                SaveChanges();
            }

            var bookingEntity = Bookings.First();

            if (!Payments.Any())
            {
                if (paymentSeedData.Count == 0)
                {
                    var random = new Random();
                    var roomLookup = Rooms.ToDictionary(r => r.RoomID, r => r);

                    foreach (var booking in Bookings.Where(b => b.HotelID == hotelEntity.HotelID))
                    {
                        var nights = Math.Max(1, (booking.CheckOutDate - booking.CheckInDate).Days);
                        if (!roomLookup.TryGetValue(booking.RoomID, out var room))
                        {
                            room = roomEntity;
                        }

                        var method = random.Next(0, 3) switch
                        {
                            0 => "CreditCard",
                            1 => "BankTransfer",
                            _ => "Cash"
                        };

                        var total = room.PricePerNight * nights;
                        var paymentDate = booking.CheckOutDate.AddDays(random.Next(0, 3));

                        paymentSeedData.Add((booking, total, paymentDate, method));
                    }
                }

                var payments = paymentSeedData.Select(data => new Payment
                {
                    BookingID = data.booking.BookingID,
                    TotalPayment = Math.Round(data.total, 2),
                    Method = data.method,
                    PaymentDate = data.paymentDate
                }).ToList();

                Payments.AddRange(payments);
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

        private void EnsureReviewReplyColumn()
        {
            var connection = Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA table_info('Reviews');";
                using var reader = command.ExecuteReader();

                var hasColumn = false;
                while (reader.Read())
                {
                    if (reader.FieldCount > 1)
                    {
                        var columnName = reader.GetString(1);
                        if (string.Equals(columnName, "AdminReply", StringComparison.OrdinalIgnoreCase))
                        {
                            hasColumn = true;
                            break;
                        }
                    }
                }

                if (!hasColumn)
                {
                    using var alter = connection.CreateCommand();
                    alter.CommandText = "ALTER TABLE Reviews ADD COLUMN AdminReply TEXT";
                    alter.ExecuteNonQuery();
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }


    }
}
