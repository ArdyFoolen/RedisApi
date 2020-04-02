using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RedisApi.Helpers
{
    public class XmlSerializer : ISerializer
    {
        public byte[] Serialize<T>(T data)
            where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream))
                {
                    serializer.Serialize(xmlWriter, data);

                    return memoryStream.ToArray();
                }
            }
        }

        public T Deserialize<T>(byte[] data)
            where T : class
        {
            if (data == null || data.Length == 0)
            {
                throw new InvalidOperationException();
            }

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }
    }
}
