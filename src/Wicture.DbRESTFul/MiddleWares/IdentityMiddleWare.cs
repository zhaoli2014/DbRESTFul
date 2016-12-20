using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 用户身份中间件，CSI中如果需要用到当前用户的userId，userName等作为参数，而调用者无法传入的时候，可以通过该中间件处理。
    /// </summary>
    public class IdentityMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.BeforeBuildCommandParam; } }
        public string Name { get { return "identity"; } }

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
        /// Parse the middle ware to parameters.
        /// </summary>
        /// <param name="config">
        /// {
        ///     "userId": "operator",
        ///     "userName": "userName",
        ///     "role": "role"
        /// }
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            var param = context.Param as JObject;
            var identity = config as JObject;

            var kuid = identity.Value<string>("userId");
            param[string.IsNullOrEmpty(kuid) ? "userId" : kuid] = context.Repository.CurrentUser.Id;

            var kun = identity.Value<string>("userName");
            param[string.IsNullOrEmpty(kun) ? "userName" : kun] = context.Repository.CurrentUser.Name;

            var kur = identity.Value<string>("role");
            param[string.IsNullOrEmpty(kur) ? "role" : kur] = context.Repository.CurrentUser.Role;
        }        
    }
}