using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Linq;

namespace Wicture.DbRESTFul.Redis
{
    public class RedisContext
    {
        private RedisConnectionManager connectionManager;
        public RedisContext()
        {
            connectionManager = new RedisConnectionManager();
        }

        public object StringGet(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);
                var data = db.StringGet(model.key);

                if (model.resultType == ResultType.Object) return JObject.Parse(data.ToString());
                else if(model.resultType == ResultType.String) return data.ToString();

                return data;
            }
        }

        public object StringSet(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);

                var val = (model.param is string || !model.param.GetType().IsByRef) ? model.param : JsonConvert.SerializeObject(model.param);

                return new { success = db.StringSet(model.key, val.ToString()) };
            }
        }

        public object ListPop(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);
                var data = db.ListLeftPop(model.key);

                if (model.resultType == ResultType.Object) return JToken.Parse(data.ToString());
                else if (model.resultType == ResultType.String) return data.ToString();

                return data;
            }
        }

        public object ListRange(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);
                var data = db.ListRange(model.key);

                if (model.resultType == ResultType.Object) return data.Select(v => JToken.Parse(v.ToString()));
                else if (model.resultType == ResultType.String) return data.Select(v => v.ToString());

                return data;
            }
        }

        public object ListPush(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);
                var val = (model.param is string || !model.param.GetType().IsByRef) ? model.param : JsonConvert.SerializeObject(model.param);
                var result = db.ListRightPush(model.key, val.ToString());

                return new { length = result };
            }
        }

        public object KeyDelete(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);
                return new { success = db.KeyDelete(model.key) };
            }
        }

        public object SetAdd(CRIModel model)
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionManager.ConnectionString))
            {
                IDatabase db = redis.GetDatabase(model.dbIndex);

                var val = (model.param is string || !model.param.GetType().IsByRef) ? model.param : JsonConvert.SerializeObject(model.param);

                return new { success = db.SetAdd(model.key, val.ToString()) };
            }
        }
    }
}
