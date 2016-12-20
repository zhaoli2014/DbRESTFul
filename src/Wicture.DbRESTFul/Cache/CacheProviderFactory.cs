using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.Cache
{
    public static class CacheProviderFactory
    {
        public static Dictionary<string, ICacheProvider> CacheProviders { get; set; } = new Dictionary<string, ICacheProvider>();

        public static void Init()
        {
            var setting = ConfigurationManager.Settings?.API.CacheConfigs?.FirstOrDefault(c => c.Type == RedisCacheProvider.Name);
            if (setting != null)
            {
                var connectionString = setting.Config.Value<string>("ConnectionString");
                var databaseIndex = setting.Config.Value<int>("DatabaseIndex");

                var redisCacheProvider = new RedisCacheProvider(connectionString, databaseIndex);
                CacheProviders[RedisCacheProvider.Name] = redisCacheProvider;
            }
        }

        public static void Set(string type, string key, object data, int expiration)
        {
            if(CacheProviders.ContainsKey(type))
            {
                CacheProviders[type].Set(key, data, expiration);
            }
        }

        public static JToken Get(string type, string key)
        {
            if (CacheProviders.ContainsKey(type))
            {
                return CacheProviders[type].Get(key);
            }

            return null;
        }
    }
}
