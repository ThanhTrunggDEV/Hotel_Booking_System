using Hotel_Manager.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.Interfaces
{
    interface IRepository<T>
    {
        
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task AddAsync(T data);
        Task UpdateAsync(T data);
        Task DeleteAsync(string id);
        Task SaveAsync();
        
    }
}
