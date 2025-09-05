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
    class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;
        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
        }

        public async Task DeleteAsync(string id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.RoomID == id);
            if (room == null) return;
            _context.Rooms.Remove(room);
        }

        public Task<List<Room>> GetAllAsync() => _context.Rooms.ToListAsync();

        public Task<Room?> GetByIdAsync(string id) => _context.Rooms.FirstOrDefaultAsync(r => r.RoomID == id);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await SaveAsync();
        }
    }
}
