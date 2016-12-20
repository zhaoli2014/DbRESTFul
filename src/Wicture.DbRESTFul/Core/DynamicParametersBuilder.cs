using Dapper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Wicture.DbRESTFul
{
    public static class DynamicParametersBuilder
    {
        public static object Build(JToken param)
        {
            if (param == null)
            {
                throw new ArgumentNullException("param");
            }

            if (param is JObject)
            {
                return new DynamicParameters(BuildParamForJObject(param as JObject));
            }
            else if(param is JArray)
            {
                var result = new List<DynamicParameters>();
                foreach (var item in param as JArray)
                {
                    result.Add(new DynamicParameters(BuildParamForJObject(item as JObject)));
                }

                return result;
            }

            throw new Exception("Unsupported param type: {0}".FormatWith(param.GetType()));
        }

        private static object BuildParamForJObject(JObject param)
        { 
            var data = new ExpandoObject() as IDictionary<string, object>;

            foreach (var item in param)
            {
                object val = null;

                if (item.Value is JArray)
                {
                    val = (item.Value as JArray).Values();
                }
                else
                {
                    val = (item.Value as JValue).Value;
                }

                data.Add(item.Key, val);
            }

            return data;
        }
    }
}