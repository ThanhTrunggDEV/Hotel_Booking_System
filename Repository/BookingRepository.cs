using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;
using Hotel_Booking_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Booking_System.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Booking data)
        {
            await _context.Bookings.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.BookingID == id);
            if (booking != null)
            _context.Bookings.Remove(booking);
        }

        public Task<List<Booking>> GetAllAsync() => _context.Bookings.ToListAsync();

        public Task<List<Booking>> GetBookingByUserId(string userId) => _context.Bookings.Where(b => b.UserID == userId).ToListAsync();

        public Task<Booking?> GetByIdAsync(string id) => _context.Bookings.FirstOrDefaultAsync(r => r.BookingID == id);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking data)
        {
            _context.Bookings.Update(data);
            await SaveAsync();
        }
    }
}
