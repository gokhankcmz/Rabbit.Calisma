using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitWebApiProducer.Repository
{
    public interface IRepository<T>
    {
        Task<T> CreateAsync(T document);
        Task<T> GetByIdAsync(Guid id);
        
        Task<List<T>> GetAllAsync();
    }
}