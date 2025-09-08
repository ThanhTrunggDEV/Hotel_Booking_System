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
    internal class AIChatRepository : IAIChatRepository
    {
        private readonly AppDbContext _context;

        public AIChatRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(AIChat data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            await _context.AIChats.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
            var chat = await _context.AIChats.FirstOrDefaultAsync(c => c.ChatID == id);
            if (chat != null)
            {
                _context.AIChats.Remove(chat);
            }
        }

        public async Task<List<AIChat>> GetAllAsync()
        {
            return await _context.AIChats.ToListAsync();
        }

        public async Task<AIChat?> GetByIdAsync(string id)
        {
            return await _context.AIChats.FirstOrDefaultAsync(c => c.ChatID == id);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AIChat data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var existing = await _context.AIChats.FirstOrDefaultAsync(c => c.ChatID == data.ChatID);
            if (existing != null)
            {
                existing.UserID = data.UserID;
                existing.Message = data.Message;
                existing.Response = data.Response;
                existing.CreatedAt = data.CreatedAt;
                _context.AIChats.Update(existing);
            }
        }
    }
}
