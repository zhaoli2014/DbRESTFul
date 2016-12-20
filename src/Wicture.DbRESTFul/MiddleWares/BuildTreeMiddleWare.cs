using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.MiddleWares
{
    public class BuildTreeMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.AfterInvoke; } }
        public string Name { get { return "buildtree"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            if (!(config is JObject))
            {
                message = "config type should be an object.";
            }

            return message;
        }

        /// <summary>
        /// Parse the middle ware to result.
        /// </summary>
        /// <param name="config">
        /// "buildtree": {
        ///    "idKey": "id",
        ///    "parentIdKey": "parentId",
        ///    "childrenKey": "children",
        ///    "childrenCountKey": "childrenCount"
        ///  }
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            var childrenKey = string.IsNullOrEmpty(config.Value<string>("childrenKey")) ? "children" : config.Value<string>("childrenKey");
            var idKey = string.IsNullOrEmpty(config.Value<string>("idKey")) ? "id" : config.Value<string>("idKey");
            var parentIdKey = string.IsNullOrEmpty(config.Value<string>("parentIdKey")) ? "parentId" : config.Value<string>("parentIdKey");
            var childrenCountKey = string.IsNullOrEmpty(config.Value<string>("childrenCountKey")) ? "childrenCount" : config.Value<string>("childrenCountKey");

            context.Result = BuildTree<string> (JArray.FromObject(context.Result), childrenKey, idKey, parentIdKey, childrenCountKey);
        }

        /// <summary>
        /// build tree
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <param name="data"></param>
        /// <param name="childrenKey"></param>
        /// <param name="idKey"></param>
        /// <param name="parentIdKey"></param>
        /// <param name="childrenCountKey"></param>
        /// <returns></returns>
        protected List<JObject> BuildTree<Tkey>(JArray data, string childrenKey = "children", string idKey = "id", string parentIdKey = "parentId", string childrenCountKey = "childrenCount")
        {
            var source = data.OfType<JObject>().ToList();

            while (true)
            {
                var last = source.LastOrDefault();
                var parent = source.FirstOrDefault(o => o.Value<Tkey>(idKey).Equals(last.Value<Tkey>(parentIdKey)));

                if (parent == null)
                {
                    break;
                }

                var siblings = source.Where(o => o.Value<Tkey>(parentIdKey) != null && o.Value<Tkey>(parentIdKey).Equals(last.Value<Tkey>(parentIdKey)));
                var count = siblings.Count();
                parent[childrenKey] = JToken.FromObject(new List<JObject>(siblings));
                parent[childrenCountKey] = count;
                source.RemoveRange(source.Count - count, count);
            }

            return source;
        }
    }
}
