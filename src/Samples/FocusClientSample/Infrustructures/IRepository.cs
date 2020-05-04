using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FocusClientSample.Infrustructures
{
    public interface IRepository<T> where T : Entity
    {
        IEnumerable<T> GetAll();

        T Add(T entity);

        T Update(T entity);

        bool Remove(int id);

        T Get(int id);
    }

    internal class Repository<T> : IRepository<T> where T : Entity
    {
        private static int _newId;
        private static ConcurrentDictionary<int, T> _data = new ConcurrentDictionary<int, T>();
        
        public IEnumerable<T> GetAll()
        {
            return _data.Values.AsEnumerable();
        }

        public T Add(T entity)
        {
            entity.Id = Interlocked.Increment(ref _newId);
            while(!_data.TryAdd(entity.Id, entity))
                entity.Id = Interlocked.Increment(ref _newId);
            /*if (!_data.TryAdd(entity.Id, entity) && _data.ContainsKey(entity.Id))
                throw new Exception("Duplicate");*/
            return entity;
        }

        public T Update(T entity)
        {
            if(!_data.TryGetValue(entity.Id, out T entry))
                throw new Exception($"The data [Id={entity.Id}] want update does not exist");

            _data.TryRemove(entry.Id, out T removed);
            _data.TryAdd(entity.Id, entity);
            return entity;
        }

        public bool Remove(int id)
        {
            return _data.TryGetValue(id, out T removed);
        }

        public T Get(int id)
        {
            _data.TryGetValue(id, out T value);
            return value;
        }

    }

    public abstract class Entity
    {
        public int Id { get; set; }
    }
}
