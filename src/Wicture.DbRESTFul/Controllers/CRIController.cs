using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wicture.DbRESTFul.Infrastructure;
using Wicture.DbRESTFul.Redis;

namespace Wicture.DbRESTFul.Controllers
{
    [Route("api/v1/cri")]
    public class CRIController : Controller
    {
        private static Dictionary<string, MethodInfo> methods;

        static CRIController()
        {
            methods = typeof(RedisContext).GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .ToDictionary(mi => mi.Name);
        }

        [HttpPost]
        public object Execute()
        {
            var data = RequestDataParser.Parse(null, CRIModel.Schemas, HttpContext);

            var model = data.ToObject<CRIItem>();

            if (ConfigurationManager.Settings.CRI.ApiOnly)
            {
                throw new Exception("CRI is only allowed for API, or set 'ApiOnly' to false in config.json file.");
            }

            RedisContext redisContext = new RedisContext();

            if (!methods.ContainsKey(model.method))
            {
                throw new Exception($"The method '{model.method}' is not supproted.");
            }

            var method = methods[model.method];

            var result = method.Invoke(redisContext, new object[] { model });

            return AjaxResult.SetResult(result);
        }
    }
}
