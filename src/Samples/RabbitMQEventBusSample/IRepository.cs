using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMQEventBusSample
{
    public interface IRepository<T, TKey> 
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        IEnumerable<T> GetAll();

        T Add(T entity);

        T Update(T entity);

        bool Remove(TKey id);

        T Get(TKey id);
    }

    internal class Repository<T, TKey> : IRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        //private static int _newId;
        private static ConcurrentDictionary<TKey, T> _data = new ConcurrentDictionary<TKey, T>();

        public IEnumerable<T> GetAll()
        {
            return _data.Values.AsEnumerable();
        }

        public T Add(T entity)
        {
            if (!_data.TryAdd(entity.Id, entity))
                throw new DuplicateWaitObjectException(nameof(entity));
            //entity.Id = Interlocked.Increment(ref _newId);
            //while (!_data.TryAdd(entity.Id, entity))
            //    entity.Id = Interlocked.Increment(ref _newId);

            /*if (!_data.TryAdd(entity.Id, entity) && _data.ContainsKey(entity.Id))
                throw new Exception("Duplicate");*/
            return entity;
        }

        public T Update(T entity)
        {
            if (!_data.TryGetValue(entity.Id, out T entry))
                throw new Exception($"The data [Id={entity.Id}] want update does not exist");

            _data.TryRemove(entry.Id, out T removed);
            _data.TryAdd(entity.Id, entity);
            return entity;
        }

        public bool Remove(TKey id)
        {
            return _data.TryGetValue(id, out T removed);
        }

        public T Get(TKey id)
        {
            _data.TryGetValue(id, out T value);
            return value;
        }

    }

    public abstract class Entity<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        public TKey Id { get; set; }
    }
}
