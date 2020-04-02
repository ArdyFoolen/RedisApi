using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisApi.Helpers
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T data)
            where T: class;

        T Deserialize<T>(byte[] data)
            where T : class;
    }
}
