using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace RedisApi.Common
{
    public interface IRedisCacheProviderAsync
    {
        Task SetAsync<T>(string key, T value)
            where T : class;

        Task SetAsync<T>(string key, T value, TimeSpan timeout)
            where T : class;

        Task<T> GetAsync<T>(string key)
            where T : class;

        Task<bool> RemoveAsync(string key);

        Task<bool> KeyExistsAsync(RedisKey key);
    }
}
