using Hotel_Booking_System.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.Interfaces
{
    public interface IRepository<T>
    {
        
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task AddAsync(T data);
        Task UpdateAsync(T data);
        Task DeleteAsync(string id);
        Task SaveAsync();
        
    }
}
