using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using RedisApi.Configuration;
using RedisApi.Helpers;
using StackExchange.Redis;

// Install Server and Redis-Cli instructions:
//      https://medium.com/@binary10111010/redis-cli-installation-on-windows-684fb6b6ac6b
// Download Redis React
//      https://github.com/ServiceStackApps/RedisReact
//          1) Go to Windows under Download
//          2) Click RedisReact-winforms.exe
// Intro tutorial
//      https://redis.io/topics/data-types-intro

namespace RedisApi.Common
{
    public class RedisCacheProvider : IRedisCacheProviderAsync, IDisposable
    {
        #region Privates

        private ConnectionMultiplexer redis;
        private IDatabase database;
        private ISerializer serializer;

        #endregion

        #region ctors

        public RedisCacheProvider(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        #endregion

        #region Connect

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public bool IsConnected => redis != null && redis.IsConnected;

        public async Task ConnectAsync()
        {
            if (!IsConnected)
            {
                await Semaphore.WaitAsync();

                try
                {
                    if (!IsConnected)
                    {
                        var endPoint = new DnsEndPoint(RedisConfigurationManager.Config.Host, RedisConfigurationManager.Config.Port, AddressFamily.InterNetwork);
                        var options = new ConfigurationOptions
                        {
                            AbortOnConnectFail = false,
                            ConnectRetry = 3,
                            ConnectTimeout = 5000,
                            SyncTimeout = 5000,
                            DefaultDatabase = 0,
                            KeepAlive = 60,
                            EndPoints = { { endPoint } }
                        };
                        redis = await ConnectionMultiplexer.ConnectAsync(options);
                        //redis.PreserveAsyncOrder = false;
                        database = redis.GetDatabase();
                    }
                }
                catch (Exception)
                {
                    redis = null;
                }
                finally
                {
                    Semaphore.Release();
                }
            }
        }

        #endregion

        #region Keys

        public async Task<long> KeyDeleteAsync(RedisKey key)
        {
            return await KeyDeleteAsync(new RedisKey[] { key });
        }

        public async Task<long> KeyDeleteAsync(params RedisKey[] keys)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyDeleteAsync(keys);
            }
            return 0;
        }

        public async Task<bool> KeyExistsAsync(RedisKey key)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyExistsAsync(key);
            }
            return false;
        }

        public async Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyExpireAsync(key, expiry);
            }
            return false;
        }

        public async Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyExpireAsync(key, expiry);
            }
            return false;
        }

        public async Task<bool> KeyPersistAsync(RedisKey key)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyPersistAsync(key);
            }
            return false;
        }

        public async Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyRenameAsync(key, newKey);
            }
            return false;
        }

        public async Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyTimeToLiveAsync(key);
            }
            return null;
        }

        public async Task<RedisType> KeyTypeAsync<T>(string key)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                var value = await database.KeyTypeAsync(key);
            }
            return RedisType.None;
        }

        #endregion

        public async Task<T> GetAsync<T>(string key)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                var value = await database.StringGetAsync(key);
                return serializer.Deserialize<T>(value);
            }
            return null;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.KeyDeleteAsync(key);
            }
            return false;
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
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue rValue = serializer.Serialize<T>(value);
                await database.StringSetAsync(key, rValue, timeout);
            }
        }

        public async Task<T[]> SetMembersAsync<T>(RedisKey key)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue[] rValues = await database.SetMembersAsync(key);
                T[] values = new T[rValues.Length];
                for (int i = 0; i < rValues.Length; i++)
                    values[i] = serializer.Deserialize<T>(rValues[i]);
                return values;
            }
            return null;
        }

        public async Task SetAddAsync<T>(string key, T value)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue rValue = serializer.Serialize<T>(value);
                await database.SetAddAsync(key, rValue);
            }
        }

        public async Task SetAddAsync(string key, string value)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                await database.SetAddAsync(key, value);
            }
        }

        public async Task<long> SetRemoveAsync<T>(RedisKey key, params T[] values)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue[] rValues = new RedisValue[values.Length];
                for (int i = 0; i < values.Length; i++)
                    rValues[i] = serializer.Serialize<T>(values[i]);

                return await database.SetRemoveAsync(key, rValues);
            }
            return 0;
        }

        public async Task<bool> InSetAsync<T>(string key, T value)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue rValue = serializer.Serialize<T>(value);
                return await database.SetContainsAsync(key, rValue);
            }
            return false;
        }

        public async Task<bool> InSetAsync(string key, string value)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.SetContainsAsync(key, value);
            }
            return false;
        }

        public async Task<T> SortedSetGetAsync<T>(string key, double score)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                var rValues = await database.SortedSetRangeByScoreAsync(key, score, score);
                return rValues == null || rValues.Length == 0 ? null : serializer.Deserialize<T>(rValues[0]);
            }
            return null;
        }

        public async Task<T[]> SortedSetRangeAsync<T>(string key)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                var rValues = (await database.SortedSetRangeByScoreAsync(key));
                T[] values = new T[rValues.Length];
                for (int i = 0; i < rValues.Length; i++)
                    values[i] = serializer.Deserialize<T>(rValues[i]);

                return values;
            }
            return null;
        }

        public async Task<long> SortedSetRemoveAsync<T>(string key, double score)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return await database.SortedSetRemoveRangeByScoreAsync(key, score, score);
            }
            return 0;
        }

        public async Task SortedSetAddAsync<T>(string key, double score, T value)
            where T : class
        {
            await ConnectAsync();

            if (IsConnected)
            {
                RedisValue rValue = serializer.Serialize<T>(value);
                await database.SortedSetAddAsync(key, rValue, score);
            }
        }

        public async Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                ISubscriber subscriber = this.redis.GetSubscriber();
                subscriber.Subscribe(channel, handler);
            }
        }

        public async Task SubscribePatternAsync(string pattern, Action<RedisChannel, RedisValue> handler)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                ISubscriber subscriber = this.redis.GetSubscriber();
                var redisChannel = new RedisChannel(pattern, RedisChannel.PatternMode.Pattern);
                subscriber.Subscribe(redisChannel, handler);
            }
        }

        public async Task UnsubscribeAsync(string channel)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                ISubscriber subscriber = this.redis.GetSubscriber();
                subscriber.Unsubscribe(channel);
            }
        }

        public async Task PublishAsync(string channel, string message)
        {
            await ConnectAsync();

            if (IsConnected)
            {
                ISubscriber subscriber = this.redis.GetSubscriber();
                subscriber.Publish(channel, message);
            }
        }

        public async Task<IRedisCacheProviderAsync> CreateBatch()
        {
            await ConnectAsync();

            if (IsConnected)
            {
                return new RedisBatchProvider(this.serializer, this.database.CreateBatch());
            }
            return null;
        }

        public void Test()
        {
        }

        #region Dispose pattern

        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
                this.redis.Close();
            }

            disposed = true;
        }

        #endregion
    }
}
