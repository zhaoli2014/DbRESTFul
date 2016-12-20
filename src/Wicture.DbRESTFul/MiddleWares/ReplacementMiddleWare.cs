using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 参数替换中间件，即CSI调用中，因为Dapper有时并不能将所有"@paramname"的形式作为参数处理，比如：
    /// @tablename, @in, @orderby，这些我们可能希望作为一个类似参数来处理，但Dapper不支持这样做。
    /// 通过此中间件，可以将指定的"@paramname"的形式作为参数替换处理。
    /// </summary>
    public class ReplacementMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.BeforeInvoke; } }
        public string Name { get { return "replace"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            if (!(config is JArray))
            {
                message = "config type should be a string array.";
            }

            if (!(context.Param is JObject))
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "parameter type should be an object.";
            }

            return message;
        }

        /// <summary>
        /// Parse the diddle ware to Validator dictionary.
        /// </summary>
        /// <param name="config">An string array to indicate the sql in the code with parameter name will be replaced by param value before invocation.
        /// [ "orderBy", "dbName" ] 
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            if (context.Param != null && context.Param is JObject)
            {
                var jObj = context.Param as JObject;
                var replaceParameters = (config as JArray).Select(i => i.ToString());

                if (context.CodeItem.code is JValue)
                {
                    context.CodeItem.code = ReplaceCodeParameters(jObj, replaceParameters, context.CodeItem.code as JValue);
                }
                else if (context.CodeItem.code is JObject)
                {
                    var code = context.CodeItem.code as JObject;
                    foreach (var item in code)
                    {
                        if (item.Value is JValue)
                        {
                            code[item.Key] = ReplaceCodeParameters(jObj, replaceParameters, item.Value as JValue);
                        }
                    }
                }
            }
        }

        private static string ReplaceCodeParameters(JObject jObj, IEnumerable<string> replaceParameters, JValue code)
        {
            var result = code.ToString();

            // Process OrderBy.
            var orderBy = jObj.Value<string>("orderBy") ?? string.Empty;
            if (!string.IsNullOrEmpty(orderBy))
            {
                result = result.Replace("@orderBy", "ORDER BY " + orderBy);
            }

            foreach (var item in replaceParameters)
            {
                if (jObj[item].Type != JTokenType.Array)
                {
                    result = result.Replace("@" + item, jObj.Value<string>(item));
                }
            }

            return result;
        }
    }
}