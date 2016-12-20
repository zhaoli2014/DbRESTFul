using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul.Cache
{
    /// <summary>
    /// 缓存服务实现接口
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <param name="key">指定的键</param>
        JToken Get(string key);

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <param name="key">指定的键</param>
        /// <param name="data">指定的数据</param>
        /// <param name="expiration">指定过期时间（以秒为单位）</param>
        void Set(string key, object data, int expiration);
    }
}