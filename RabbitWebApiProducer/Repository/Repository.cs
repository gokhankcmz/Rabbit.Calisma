using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace RabbitWebApiProducer.Repository
{
    public class Repository<T> : IRepository<T> where T : Document
    {
        private readonly IMongoCollection<T> _collection;

        public Repository(IMongoClient client, MongoSettings settings)
        {
            var collectionName = typeof(T).Name;
            collectionName = collectionName[0].ToString().ToLowerInvariant() + collectionName[1..];
            _collection = client.GetDatabase(settings.DatabaseName).GetCollection<T>(collectionName);
        }
        
        public async Task<T> CreateAsync(T document)
        {
            document.CreatedAt = DateTime.Now;
            document.UpdatedAt = DateTime.Now;
            await _collection.InsertOneAsync(document);
            return document;
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _collection.AsQueryable().FirstOrDefaultAsync(x => x.Id.Equals(id));
        }


        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.AsQueryable().ToListAsync();
        }

        
    }
}