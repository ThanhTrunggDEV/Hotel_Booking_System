using Hotel_Manager.DomainModels;
using Hotel_Manager.Interfaces;
using Hotel_Manager.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Repository
{
    class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }
        public async Task AddAsync(User data)
        {
            await _context.Users.AddAsync(data);
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (user == null) return;
            _context.Users.Remove(user);
        }
     

        public Task<List<User>> GetAllAsync() => _context.Users.ToListAsync();

        public Task<User?> GetByIdAsync(string id) => _context.Users.FirstOrDefaultAsync(u => u.UserID == id);

        public Task<User?> GetByUsernameAsync(string username) => _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User data)
        {
            _context.Users.Update(data);
            await SaveAsync();
        }
    }
}
