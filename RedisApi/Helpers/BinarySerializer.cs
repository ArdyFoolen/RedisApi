using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RedisApi.Helpers
{
    public class BinarySerializer : ISerializer
    {
        public byte[] Serialize<T>(T data)
            where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
            where T : class
        {
            if (data == null || data.Length == 0)
            {
                throw new InvalidOperationException();
            }

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }
}
