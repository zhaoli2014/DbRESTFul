using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace Wicture.DbRESTFul
{
    public enum ResultType
    {
        Object,
        String
    }

    public class CRIModel
    {
        public RedisKey key { get; set; }
        public string method { get; set; }
        public int dbIndex { get; set; } = 0;
        public ResultType resultType { get; set; } = ResultType.String;
        public object param { get; set; }

        public static readonly Schema[] Schemas = new[]
        {
            new Schema { name = "key", nullable = false },
            new Schema { name = "method", nullable = false },
            new Schema { name = "dbIndex", nullable = true },
            new Schema { name = "resultType", nullable = true },
            new Schema { name = "param", nullable = true },
        };
    }

    public class CRIItem : CRIModel
    {
        public string name { get; set; }

        public CRIItem Clone()
        {
            return new CRIItem
            {
                name = this.name,
                key = this.key,
                method = this.method,
                dbIndex = this.dbIndex,
                resultType = this.resultType
            };
        }
    }
}
