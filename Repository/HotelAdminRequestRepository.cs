using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.DomainModels;
using Hotel_Booking_System.Interfaces;

namespace Hotel_Booking_System.Repository
{
    class HotelAdminRequestRepository : IHotelAdminRequestRepository
    {
        private readonly AppDbContext _context;
        public async Task AddAsync(HotelAdminRequest data)
        {
            await _context.HotelAdminRequests.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
            var request = await  _context.HotelAdminRequests.FirstOrDefaultAsync(x => x.RequestID == id);
            if (request != null) _context.HotelAdminRequests.Remove(request);

        }

        public Task<List<HotelAdminRequest>> GetAllAsync() => _context.HotelAdminRequests.ToListAsync();

        public Task<HotelAdminRequest?> GetByIdAsync(string id) => _context.HotelAdminRequests.FirstOrDefaultAsync(u => u.RequestID == id);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HotelAdminRequest data)
        {
            await _context.HotelAdminRequests.UpdateAsync(data);
            await SaveAsync();
        }
    }
}
