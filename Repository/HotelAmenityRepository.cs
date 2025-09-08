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
    internal class HotelAmenityRepository : IHotelAmenityRepository
    {
        private readonly AppDbContext _context;

        public HotelAmenityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HotelAmenity data)
        {
            await _context.HotelAmenities.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
            var amenity = await _context.HotelAmenities.FindAsync(id);
            if (amenity != null)
            {
                _context.HotelAmenities.Remove(amenity);
            }
        }

        public async Task<List<HotelAmenity>> GetAllAsync()
        {
            return await _context.HotelAmenities.ToListAsync();
        }

        public async Task<HotelAmenity?> GetByIdAsync(string id)
        {
            return await _context.HotelAmenities.FindAsync(id);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HotelAmenity data)
        {
            _context.HotelAmenities.Update(data);
            await SaveAsync();
        }
    }
}
