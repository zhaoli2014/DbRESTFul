using Newtonsoft.Json.Linq;
using System;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 默认值中间件，对于前端那些用户输入需要考虑默认值的情况，可以通过该中间件处理。
    /// </summary>
    public class DefaultValueMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.BeforeBuildCommandParam; } }
        public string Name { get { return "defaults"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            if (!(config is JObject))
            {
                message = "config type should be an object.";
            }

            if (!(context.Param is JObject))
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "parameter type should be an object.";
            }

            return message;
        }

        /// <summary>
        /// Parse the middle ware to param.
        /// </summary>
        /// <param name="config">
        /// {
        ///     "name": "unknown",
        ///     "age": 32
        /// }
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            var param = context.Param as JObject;
            var defaults = config as JObject;
            foreach (var item in defaults)
            {
                if (param[item.Key] == null || param[item.Key].ToString() == string.Empty)
                {
                    param[item.Key] = item.Value;
                }
            }
        }        
    }
}