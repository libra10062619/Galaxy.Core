using System;
using System.Net;
using StackExchange.Redis;

namespace Galaxy.Core.Caching.Redis
{
    public interface IRedisConnectionWrapper
    {
        /// <summary>
        /// Obtain an interactive connection to a database inside redis
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        /// <returns>Redis cache database</returns>
        IDatabase GetDatabase(int? db = null);

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        /// <param name="endPoint">The network endpoint</param>
        /// <returns>Redis server</returns>
        IServer GetServer(EndPoint endPoint);

        string GetInstanceName();

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns>Array of endpoints</returns>
        EndPoint[] GetEndPoints();

        /// <summary>
        /// Delete all the keys of the database
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value<</param>
        void FlushDatabase(int? db = null);
    }
}
