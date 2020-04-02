//using RedisApi.Configuration;
//using ServiceStack.Redis;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace RedisApi.Common
//{
//    public class RedisCacheProvider : ICacheProvider
//    {
//        RedisEndpoint _endPoint;

//        public RedisCacheProvider()
//        {
//            _endPoint = new RedisEndpoint(RedisConfigurationManager.Config.Host, RedisConfigurationManager.Config.Port,
//                RedisConfigurationManager.Config.Password, RedisConfigurationManager.Config.DatabaseID);
//        }

//        public void Set<T>(string key, T value)
//            where T : class
//        {
//            this.Set(key, value, TimeSpan.Zero);
//        }

//        public void Set<T>(string key, T value, TimeSpan timeout)
//            where T : class
//        {
//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                client.As<T>().SetValue(key, value, timeout);
//            }
//        }

//        public T Get<T>(string key)
//            where T : class
//        {
//            T result = default(T);

//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                var wrapper = client.As<T>();

//                result = wrapper.GetValue(key);
//            }

//            return result;
//        }

//        public bool Remove(string key)
//        {
//            bool removed = false;

//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                removed = client.Remove(key);
//            }

//            return removed;
//        }

//        public bool IsInCache(string key)
//        {
//            bool isInCache = false;

//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                isInCache = client.ContainsKey(key);
//            }

//            return isInCache;
//        }

//        public List<string> Keys()
//        {
//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                return client.GetAllKeys();
//            }
//        }

//        public string Type(string key)
//        {
//            using (RedisClient client = new RedisClient(_endPoint))
//            {
//                return client.Type(key);
//            }
//        }
//    }
//}
