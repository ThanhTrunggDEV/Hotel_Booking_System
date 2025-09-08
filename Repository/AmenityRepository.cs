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
    public class AmenityRepository : IAmenityRepository
    {
        private readonly AppDbContext _context;
        public AmenityRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Amenity data)
        {
            await _context.Amenities.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
           var data = await _context.Amenities.FindAsync(id);
            if (data != null)
                _context.Amenities.Remove(data);
        }

        public Task<List<Amenity>> GetAllAsync() => _context.Amenities.ToListAsync();

        public Task<Amenity?> GetByIdAsync(string id) => _context.Amenities.FirstOrDefaultAsync(r => r.AmenityID == id);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Amenity data)
        {
            _context.Amenities.Update(data);
            await SaveAsync();
        }
    }
}
