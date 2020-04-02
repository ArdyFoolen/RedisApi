using RedisApi.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisApi.Common
{
    public class RedisBatchProvider : IRedisCacheProviderAsync, IBatchProvider
    {
        private ISerializer serializer;
        private IBatch batch;

        public RedisBatchProvider(ISerializer serializer, IBatch batch)
        {
            this.serializer = serializer;
            this.batch = batch;
        }

        public async Task<T> GetAsync<T>(string key)
            where T : class
        {
            var value = await batch.StringGetAsync(key);
            return serializer.Deserialize<T>(value);
        }

        public async Task<bool> KeyExistsAsync(RedisKey key)
        {
            return await batch.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await batch.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value)
            where T : class
        {
            await this.SetWithTimeoutAsync(key, value, null);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan timeout)
            where T : class
        {
            await this.SetWithTimeoutAsync(key, value, timeout);
        }

        private async Task SetWithTimeoutAsync<T>(string key, T value, TimeSpan? timeout)
            where T : class
        {
            RedisValue rValue = serializer.Serialize<T>(value);
            await batch.StringSetAsync(key, rValue, timeout);
        }

        public void Execute()
        {
            batch.Execute();
        }

    }
}
