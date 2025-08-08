using Hotel_Manager.DomainModels;
using Hotel_Manager.Interfaces;
using Hotel_Manager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Repository
{
    class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;
        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task AddAsync(Room room)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Room>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Room?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Room room)
        {
            throw new NotImplementedException();
        }
    }
}
