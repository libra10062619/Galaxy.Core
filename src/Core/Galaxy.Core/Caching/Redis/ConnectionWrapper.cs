using System;
using System.Net;
using StackExchange.Redis;

namespace Galaxy.Core.Caching.Redis
{
    public class RedisConnectionWrapper : IRedisConnectionWrapper
    {
        readonly string _connectionString;
        readonly string _instanceName;

        volatile ConnectionMultiplexer _connection;
        readonly object _lock = new object();

        #region Ctor

        public RedisConnectionWrapper(RedisCacheOptions options)
        {
            _connectionString = options.Configuration;
            _instanceName = options.InstanceName;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get connection to Redis servers
        /// </summary>
        /// <returns></returns>
        protected ConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected) return _connection;

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected) return _connection;

                if (_connection != null)
                {
                    //Connection disconnected. Disposing connection...
                    _connection.Dispose();
                }

                //Creating new instance of Redis Connection
                _connection = ConnectionMultiplexer.Connect(_connectionString);
            }

            return _connection;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Obtain an interactive connection to a database inside redis
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        /// <returns>Redis cache database</returns>
        public IDatabase GetDatabase(int? db = null)
        {
            return GetConnection().GetDatabase(db ?? -1); //_settings.DefaultDb);
        }

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        /// <param name="endPoint">The network endpoint</param>
        /// <returns>Redis server</returns>
        public IServer GetServer(EndPoint endPoint)
        {
            return GetConnection().GetServer(endPoint);
        }

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns>Array of endpoints</returns>
        public EndPoint[] GetEndPoints()
        {
            return GetConnection().GetEndPoints();
        }

        /// <summary>
        /// Delete all the keys of the database
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        public void FlushDatabase(int? db = null)
        {
            var endPoints = GetEndPoints();

            foreach (var endPoint in endPoints)
            {
                GetServer(endPoint).FlushDatabase(db ?? -1); //_settings.DefaultDb);
            }
        }

        public string GetInstanceName() => _instanceName;


        /// <summary>
        /// Release all resources associated with this object
        /// </summary>
        public void Dispose()
        {
            //dispose ConnectionMultiplexer
            if (_connection != null)
                _connection.Dispose();
        }

        #endregion
    }
}