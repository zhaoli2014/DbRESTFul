using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Wicture.DbRESTFul.Redis;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul
{
    public static class RedisInvoker
    {
        private static Dictionary<string, MethodInfo> Methods { get; set; }

        private static RedisContext redisContext = new RedisContext();

        static RedisInvoker()
        {
            Methods = new Dictionary<string, MethodInfo>();

            var contextType = redisContext.GetType();
            foreach (var item in contextType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                Methods.Add(item.Name, item);
            }
        }

        public static object Call(string name, JToken param)
        {
            if (!ServiceResourceManager.CRIs.ContainsKey(name))
            {
                throw new Exception($"No CRI found with name: '{name}'.");
            }
            var cri = ServiceResourceManager.CRIs[name].Clone();

            if (!Methods.ContainsKey(cri.method))
            {
                throw new Exception($"Not supported redis method '{cri.method}'.");
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.Settings.CRI.UnwrapParameterName)
                && param[ConfigurationManager.Settings.CRI.UnwrapParameterName] != null)
            {
                cri.param = param[ConfigurationManager.Settings.CRI.UnwrapParameterName];
            }
            else
            {
                cri.param = param;
            }

            var method = Methods[cri.method];
            return method.Invoke(redisContext, new object[] { cri });
        }
    }
}
