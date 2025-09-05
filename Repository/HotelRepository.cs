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
    class HotelRepository : IHotelRepository
    {
        private readonly AppDbContext _context;
        public async Task AddAsync(Hotel data)
        {
             await _context.Hotels.AddAsync(data);
        }

        public HotelRepository(AppDbContext context)
        {
            _context = context;
        }
        

        public async Task DeleteAsync(string id)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.HotelID == id);
            if (hotel == null) return;
            _context.Hotels.Remove(hotel);
        }

        public Task<List<Hotel>> GetAllAsync() => _context.Hotels.ToListAsync();

        public Task<Hotel?> GetByIdAsync(string id) => _context.Hotels.FirstOrDefaultAsync(r => r.HotelID == id);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
            await SaveAsync();
        }
    }
}
