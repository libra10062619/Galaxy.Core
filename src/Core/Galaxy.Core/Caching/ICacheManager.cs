using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Galaxy.Core.Caching
{
    public interface ICacheManager : IDisposable
    {
        #region K-V
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T data, TimeSpan? expiry = null);
        #endregion

        #region Hash
        Task<T> HashGetAsync<T>(string key, string hashField);
        Task HashSetAsync<T>(string key, string hashField, T data, TimeSpan? expiry = null);
        #endregion

        #region Entity stored as hash mode
        Task<T> HashGetEntityAsync<T>(string key, T data);
        Task HashSetEntityAsync<T>(string key, T data, TimeSpan? expiry = null);
        #endregion

        #region Entity collection stored as hash mode
        Task<IList<T>> CollectionGetAsync<T>(string key, params string[] keyPattern);
        Task CollectionSetAsync<T>(string key, IEnumerable<T> data, Func<T, string> keyPattern, TimeSpan? expiry = null);
        #endregion

        #region List
        Task<long> ListLeftPushAsync<T>(string key, T data);
        Task<long> ListLeftPushAsync<T>(string key, IEnumerable<T> data);
        Task<T> ListLeftPopAsync<T>(string key);
        Task<long> ListRightPushAsync<T>(string key, T data);
        Task<long> ListRightPushAsync<T>(string key, IEnumerable<T> data);
        Task<T> ListRightPopAsync<T>(string key);
        Task<T> ListRightPopLeftPushAsync<T>(string source, string destination);
        #endregion

        #region Set
        Task<bool> SetAddAsync<T>(string key, T value);
        Task<long> SetAddAsync<T>(string key, IEnumerable<T> values);
        Task<IList<T>> SetMembersAsync<T>(string key);
        #endregion

        Task KeyExpireAsync(string key, TimeSpan expiry);
        Task<bool> KeyExistsAsync(string key);

        Task<bool> LockTakeAsync(string key, string token, TimeSpan expiry);

        Task<bool> LockReleaseAsync(string key, string token);

        Task<long> HashIncrementAsync(string key, string hashField);
    }
}
