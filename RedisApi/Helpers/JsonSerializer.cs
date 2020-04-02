using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisApi.Helpers
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data)
            where T : class
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
        }

        public T Deserialize<T>(byte[] data)
            where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(data));
        }
    }
}
