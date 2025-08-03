using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncRepositoryNamespace
{
    public interface IAsyncRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }

    public class AsyncRepository<T> : IAsyncRepository<T> where T : class, IEntity
    {
        private readonly List<T> _entities = new();
        private int _nextId = 1;

        public async Task<T?> GetByIdAsync(int id)
        {
            await Task.Delay(10); // Simulate database call
            return _entities.FirstOrDefault(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            await Task.Delay(20);
            return _entities.ToList();
        }

        public async Task<T> AddAsync(T entity)
        {
            await Task.Delay(15);
            entity.Id = _nextId++;
            _entities.Add(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            await Task.Delay(15);
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new InvalidOperationException("Entity not found");

            var index = _entities.IndexOf(existing);
            _entities[index] = entity;
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await Task.Delay(10);
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _entities.Remove(entity);
            return true;
        }
    }

    public interface IEntity
    {
        int Id { get; set; }
    }

    public class SampleEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
