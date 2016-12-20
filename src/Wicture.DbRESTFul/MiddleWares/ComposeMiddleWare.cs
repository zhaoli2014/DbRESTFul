using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 合成中间件，将多个CSI合成为一个，且安顺序，将参数合并给下一个CSI做为输入参数，将结果按指定配置返回。
    /// </summary>
    public class ComposeMiddleWare : IMiddleWare
    {
        private static Dictionary<string, KeyValuePair<MethodInfo, IParameterPreprocessor>> functionalParameterResolvers { get; set; }

        static ComposeMiddleWare()
        {
            //functionalParameterResolvers = new Dictionary<string, KeyValuePair<MethodInfo, IParameterPreprocessor>>();
            //var baseType = typeof(IParameterPreprocessor);

            //foreach (var item in ConfigurationManager.Settings.RepositoryLibs.Split(','))
            //{
            //    var assembly = Assembly.Load(new AssemblyName(item));
            //    if (assembly != null)
            //    {
            //        assembly.GetTypes()
            //            .Where(t => baseType.IsAssignableFrom(t) && t.Name != baseType.Name)
            //            .ForEach(t => LoadMethods(t));
            //    }
            //}
        }

        private static void LoadMethods(Type repositoryType)
        {
            var instant = Activator.CreateInstance(repositoryType) as IParameterPreprocessor;

            foreach (var item in repositoryType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(mi => mi.CustomAttributes.Any(a => a.AttributeType == typeof(PreprocessMethodAttribute))))
            {
                functionalParameterResolvers.Add("{0}.{1}".FormatWith(repositoryType.Name, item.Name), new KeyValuePair<MethodInfo, IParameterPreprocessor>(item, instant));
            }
        }


        public ActivePosition ActivePosition { get { return ActivePosition.BeforeBuildCommandParam; } }
        public string Name { get { return "compose"; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;

            if (!(config is JArray))
            {
                message = "config type should be an array.";
            }

            if (context.Param is JValue)
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "parameter type should be an object or array.";
            }

            return string.Empty;
        }

        public void Resolve(InvokeContext context, JToken config)
        {
            foreach (var item in (config as JArray))
            {
                var function = item.Value<string>();
                if (!functionalParameterResolvers.ContainsKey(function))
                {
                    throw new Exception("Cannot find the Functional Parameter Method: " + function);
                }

                var method = functionalParameterResolvers[function];
                var result = method.Key.Invoke(method.Value, new[] { context.Param }) as JToken;
                if (result == null)
                {
                    throw new Exception("JToken result expected for the Functional Parameter Method: " + function);
                }

                if (context.Param == null)
                {
                    context.Param = result;
                }
                else
                {
                    context.Param = context.Param.Merge(result);
                }
            }
        }
    }
}