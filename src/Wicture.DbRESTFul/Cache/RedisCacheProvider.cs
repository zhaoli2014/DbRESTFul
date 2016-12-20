using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;

namespace Wicture.DbRESTFul.Cache
{
    /// <summary>
    /// Redis 缓存实现。
    /// </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly string connectionString;
        private readonly int databaseIndex;

        public static string Name { get { return "redis"; } }

        public RedisCacheProvider(string connectionString, int databaseIndex)
        {
            this.connectionString = connectionString;
            this.databaseIndex = databaseIndex;
        }

        public void Set(string key, object data, int expiration = 5 * 60)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString))
            {
                IDatabase db = redis.GetDatabase(databaseIndex);
                db.StringSet(key, JsonConvert.SerializeObject(data), TimeSpan.FromSeconds(expiration), When.NotExists);
            }
        }

        public JToken Get(string key)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString))
            {
                IDatabase db = redis.GetDatabase(databaseIndex);
                var data = db.StringGet(key);

                return data.IsNullOrEmpty ? null : JToken.Parse(data);
            }
        }
    }
}
