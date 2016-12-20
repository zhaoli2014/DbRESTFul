using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wicture.DbRESTFul.Converters;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 参数验证中间件，对传入的参数做验证，包括类型，邮件，范围等等
    /// </summary>
    public class ValidationMiddleWare : IMiddleWare
    {
        public ActivePosition ActivePosition { get { return ActivePosition.BeforeInvoke; } }
        public string Name { get { return "validators"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            if (!(config is JObject))
            {
                message = "config type should be an object.";
            }

            if (context.Param is JValue)
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "parameter type should be an object or array.";
            }

            return message;
        }

        public void Resolve(InvokeContext context, JToken config)
        {
            var validators = ParseValidator(config as JObject);
            if (context.Param is JObject)
            {
                ValidateItem(context.Param as JObject, validators);
            }
            else if(context.Param is JArray)
            {
                foreach (var item in context.Param as JArray)
                {
                    ValidateItem(item as JObject, validators);
                }
            }
        }

        private static void ValidateItem(JObject param, Dictionary<string, List<ICSIValidator>> validators)
        {
            var keys = new List<string>();
            foreach (var item in param)
            {
                keys.Add(item.Key);
                object val = null;

                if (item.Value is JArray)
                {
                    val = (item.Value as JArray).Values();
                }
                else
                {
                    val = (item.Value as JValue).Value;

                    // Check No-required Validators
                    if (validators.ContainsKey(item.Key))
                    {
                        Validator.Check(item.Key, val, validators[item.Key]);
                    }
                }
            }

            // Check Required Validators
            foreach (var item in validators)
            {
                if (item.Value.Any(v => v is RequiredValidator) && !keys.Contains(item.Key))
                {
                    throw new CSIValidationException(string.Format("Parameter: \"{0}\" is required.", item.Key));
                }
            }
        }

        /// <summary>
        /// Parse the diddle ware to Validator dictionary.
        /// </summary>
        /// <param name="validator">
        /// {
        ///     "name": [ "required" ],
        ///     "age": [ "range:1,30", "required" ]
        /// }
        /// </param>
        private static Dictionary<string, List<ICSIValidator>> ParseValidator(JObject validator)
        {
            Dictionary<string, List<ICSIValidator>> result = new Dictionary<string, List<ICSIValidator>>();

            if (validator != null)
            {
                foreach (var v in validator)
                {
                    var validators = Validator.Parse(v.Value.ToObject<List<string>>());
                    result.Add(v.Key, validators);
                }
            }

            return result;
        }
    }
}