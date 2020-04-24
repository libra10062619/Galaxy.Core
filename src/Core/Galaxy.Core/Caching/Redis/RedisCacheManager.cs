using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Galaxy.Core.Abstractions;
using Galaxy.Core.Extensions;
using StackExchange.Redis;
using System.Text.Json;

namespace Galaxy.Core.Caching.Redis
{
    public class RedisCacheManager : DisposableObj, ICacheManager
    {
        const string NULL_VALUE = "[#NULL#]";

        readonly IRedisConnectionWrapper _connection;
        readonly IDatabase _database;
        readonly string _instanceName;

        readonly ConcurrentDictionary<Type, System.Reflection.PropertyInfo[]> _properties =
            new ConcurrentDictionary<Type, System.Reflection.PropertyInfo[]>();

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisCacheManager(IRedisConnectionWrapper connection)
        {
            _connection = connection;
            _database = _connection.GetDatabase();
            _instanceName = connection.GetInstanceName();

        }

        #region K-V

        public virtual async Task<T> GetAsync<T>(string key)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = await _database.StringGetAsync(rKey);
            return Convert2Entity<T>(rValue);
        }

        public virtual async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = Convert2RedisValue(data);
            await _database.StringSetAsync(rKey, rValue, expiry: expiry);
        }

        #endregion

        #region Hash

        public virtual async Task<T> HashGetAsync<T>(string key, string hashField)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = await _database.HashGetAsync(rKey, hashField);
            return Convert2Entity<T>(rValue);
        }
        public virtual async Task HashSetAsync<T>(string key, string hashField, T data, TimeSpan? expiry = null)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = Convert2RedisValue(data);
            await _database.HashSetAsync(rKey, hashField, rValue);
            if (expiry.HasValue)
                await _database.KeyExpireAsync(rKey, expiry);
        }

        #endregion

        #region Entity stored as hash mode

        public virtual async Task<T> HashGetEntityAsync<T>(string key, T data)
        {
            var rKey = Convert2RedisKey(key);
            var hashEntries = await _database.HashGetAllAsync(rKey);
            return await Convert2Entity<T>(hashEntries);
        }

        public virtual async Task HashSetEntityAsync<T>(string key, T data, TimeSpan? expiry = null)
        {
            var rKey = Convert2RedisKey(key);
            var hashEntries = await Convert2HashEntry(data);
            await _database.HashSetAsync(rKey, hashEntries);
            if (expiry.HasValue)
                await _database.KeyExpireAsync(rKey, expiry);
        }

        #endregion

        #region Entity collection stored as hash mode

        public async Task<IList<T>> CollectionGetAsync<T>(string key, params string[] keyPattern)
        {
            var result = new List<T>();
            foreach (var item in keyPattern)
            {
                var rKey = Convert2RedisKey($"{key}.{item}");
                var entries = await _database.HashGetAllAsync(rKey);
                if (entries.Any())
                    result.Add(await Convert2Entity<T>(entries));
            }
            return result;
        }

        public async Task CollectionSetAsync<T>(string key, IEnumerable<T> data, Func<T, string> keyPattern, TimeSpan? expiry = null)
        {
            foreach (var item in data)
            {
                var rKey = Convert2RedisKey($"{key}.{keyPattern(item)}");

                var hashEntries = await Convert2HashEntry(item);
                await _database.HashSetAsync(rKey, hashEntries);
                if (expiry.HasValue)
                    await _database.KeyExpireAsync(rKey, expiry);
            }
        }

        #endregion

        #region Set

        public async Task<bool> SetAddAsync<T>(string key, T value)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = Convert2RedisValue(value);
            return await _database.SetAddAsync(rKey, rValue);
        }

        public async Task<long> SetAddAsync<T>(string key, IEnumerable<T> values)
        {
            var rKey = Convert2RedisKey(key);
            var rValues = values.ToList().Select(p => Convert2RedisValue(p)).ToArray();
            return await _database.SetAddAsync(rKey, rValues);
        }

        public async Task<IList<T>> SetMembersAsync<T>(string key)
        {
            var rKey = Convert2RedisKey(key);
            var rValues = await _database.SetMembersAsync(rKey);
            return rValues?.Select(p => Convert2Entity<T>(p)).ToList();
        }

        #endregion

        #region List

        public async Task<long> ListLeftPushAsync<T>(string key, T data)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = Convert2RedisValue(data);
            return await _database.ListLeftPushAsync(rKey, rValue);
        }

        public async Task<long> ListLeftPushAsync<T>(string key, IEnumerable<T> data)
        {
            var rKey = Convert2RedisKey(key);
            var rValues = data.ToList().Select(p => Convert2RedisValue(p)).ToArray();
            return await _database.ListLeftPushAsync(rKey, rValues);
        }

        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = await _database.ListLeftPopAsync(rKey);
            return Convert2Entity<T>(rValue);
        }

        public async Task<long> ListRightPushAsync<T>(string key, T data)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = Convert2RedisValue(data);
            return await _database.ListRightPushAsync(rKey, rValue);
        }

        public async Task<long> ListRightPushAsync<T>(string key, IEnumerable<T> data)
        {
            var rKey = Convert2RedisKey(key);
            var rValues = data.ToList().Select(p => Convert2RedisValue(p)).ToArray();
            return await _database.ListLeftPushAsync(rKey, rValues);
        }

        public async Task<T> ListRightPopAsync<T>(string key)
        {
            var rKey = Convert2RedisKey(key);
            var rValue = await _database.ListRightPopAsync(rKey);
            return Convert2Entity<T>(rValue);
        }


        public async Task<T> ListRightPopLeftPushAsync<T>(string source, string destination)
        {
            var rSourceKey = Convert2RedisKey(source);
            var rDestinationKey = Convert2RedisKey(destination);
            var rValue = await _database.ListRightPopLeftPushAsync(rSourceKey, rDestinationKey);
            return Convert2Entity<T>(rValue);
        }

        #endregion

        #region Others

        public async Task KeyExpireAsync(string key, TimeSpan expiry)
        {
            var rKey = Convert2RedisKey(key);
            await _database.KeyExpireAsync(rKey, expiry);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            var rKey = Convert2RedisKey(key);
            return await _database.KeyExistsAsync(rKey);
        }

        #endregion

        public async Task<long> HashIncrementAsync(string key, string hashField)
        {
            var rKey = Convert2RedisKey(key);
            return await _database.HashIncrementAsync(rKey, hashField);
        }

        public async Task<bool> LockTakeAsync(string key, string token, TimeSpan expiry)
        {
            var rKey = Convert2RedisKey(key);

            return await _database.LockTakeAsync(rKey, token, expiry);
        }

        public async Task<bool> LockReleaseAsync(string key, string token)
        {
            var rKey = Convert2RedisKey(key);
            return await _database.LockReleaseAsync(rKey, token);
        }

        protected override void Disposing()
        {
        }


        protected string Convert2RedisKey(string key) => $"{_instanceName}_{key}";

        protected RedisValue Convert2RedisValue<T>(T data) => data == null ? NULL_VALUE : data.Serialize();

        protected string Convert2Key(string redisKey) => redisKey.Replace($"{_instanceName}_", "");

        protected async Task<HashEntry[]> Convert2HashEntry<T>(T data)
        {
            var type = typeof(T);
            var properties = await GetPropertyInfos(type);
            try
            {
                return properties.Select(p =>
                {
                    RedisValue value = new RedisValue(p.GetValue(data)?.Serialize());
                    return new HashEntry(p.Name, value);
                }).ToArray();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        protected async Task<T> Convert2Entity<T>(HashEntry[] hashEntries)
        {
            T entity = Activator.CreateInstance<T>();
            var type = typeof(T);
            var properties = await GetPropertyInfos(type);
            foreach(var entry in hashEntries)
            {
                if (entry.Value.HasValue && !entry.Value.Equals(NULL_VALUE))
                {
                    var prop = properties.Single(p => p.Name == entry.Name);
                    object value = Convert.ChangeType(entry.Value, prop.PropertyType);
                    prop.SetValue(entity, value, null);
                }
            }
            return entity;
        }

        protected T Convert2Entity<T>(RedisValue redisValue) => redisValue.IsNullOrEmpty || NULL_VALUE.Equals(redisValue) ? default : JsonSerializer.Deserialize<T>(redisValue);

        protected async Task<System.Reflection.PropertyInfo[]> GetPropertyInfos(Type type)
        {
            if (!_properties.ContainsKey(type))
            {
                await _lock.WaitAsync();
                try
                {
                    if (!_properties.ContainsKey(type))
                    {
                        _properties.TryAdd(type, type.GetProperties());
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }

            _properties.TryGetValue(type, out var propertyInfos);

            return propertyInfos;
        }
    }
}
