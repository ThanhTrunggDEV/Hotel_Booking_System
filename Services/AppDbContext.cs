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

            if (!Users.Any())
            {
                var superAdmin = new User
                {
                    FullName = "Super Admin",
                    Email = "superadmin@example.com",
                    Password = "123456",
                    Role = "SuperAdmin",
                    DateOfBirth = DateTime.Now
                };

                var requester = new User
                {
                    FullName = "Hotel Admin",
                    Email = "hoteladmin@example.com",
                    Password = "123456",
                    Role = "User",
                    DateOfBirth = DateTime.Now
                };

                Users.AddRange(superAdmin, requester);
                SaveChanges();

                HotelAdminRequests.Add(new HotelAdminRequest
                {
                    UserID = requester.UserID,
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
