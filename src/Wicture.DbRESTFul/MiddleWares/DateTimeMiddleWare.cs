using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 时间值中间件，对于参数需要时间处理时，可以通过该中间件完成。
    /// </summary>
    public class DateTimeMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.BeforeBuildCommandParam; } }
        public string Name { get { return "datetime"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            //if (!(config is JObject))
            //{
            //    message = "config type should be an object.";
            //}

            //if (!(context.Param is JObject))
            //{
            //    var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
            //    message += prefix + "parameter type should be an object.";
            //}

            return message;
        }

        /// <summary>
        /// Parse the middleware to DateTime.
        /// </summary>
        /// <param name="config">
        /// {
        ///     "day": "@today",
        ///     "month": "@thisMonth",
        ///     "hour": "@thatHour",
        ///     "hour": "@thatHour",
        ///     "hour": "@thatHour",
        /// }
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            //var param = context.Param as JObject;
            //var defaults = config as JObject;
            //foreach (var item in defaults)
            //{
            //    if (param[item.Key] == null || param[item.Key].ToString() == string.Empty)
            //    {
            //        param[item.Key] = item.Value;
            //    }
            //}
        }        
    }
}