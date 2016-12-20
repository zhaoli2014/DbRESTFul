using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 参数预处理中间件，将当前用户输入的参数通过指定的一系列方法的处理，最终合并回到参数列表中。
    /// 此中间件可以解决需要对参数作一些处理，加工的使用场景，比如：用户注册，需要将密码字符串加密等。
    /// </summary>
    public class PreprocessMiddleWare : IMiddleWare
    {
        private static Dictionary<string, KeyValuePair<MethodInfo, IParameterPreprocessor>> preprocessors { get; set; }

        public static void InitPreprocess(IEnumerable<Assembly> assemblies)
        {
            if (preprocessors != null)
            {
                preprocessors.Clear();
            }
            else
            {
                preprocessors = new Dictionary<string, KeyValuePair<MethodInfo, IParameterPreprocessor>>();
            }

            var baseType = typeof(IParameterPreprocessor);

            foreach (var assembly in assemblies)
            {
                if (assembly != null)
                {
                    assembly.GetTypes()
                        .Where(t => baseType.IsAssignableFrom(t) && t.Name != baseType.Name && !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsInterface)
                        .ForEach(t => LoadMethods(t));
                }
            }
        }

        private static void LoadMethods(Type repositoryType)
        {
            try
            {
                var instant = Activator.CreateInstance(repositoryType) as IParameterPreprocessor;

                foreach (var item in repositoryType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(mi => mi.CustomAttributes.Any(a => a.AttributeType == typeof(PreprocessMethodAttribute))))
                {
                    preprocessors.Add("{0}.{1}".FormatWith(repositoryType.Name, item.Name), new KeyValuePair<MethodInfo, IParameterPreprocessor>(item, instant));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Create IParameterPreprocessor class failed for type: {repositoryType}. Please make sure it has default public constructor and not abstract.", ex);
            }
        }


        public ActivePosition ActivePosition { get { return ActivePosition.BeforeBuildCommandParam; } }
        public string Name { get { return "preprocess"; } }

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
                if (!preprocessors.ContainsKey(function))
                {
                    throw new Exception("Cannot find the Functional Parameter Method: " + function);
                }

                var method = preprocessors[function];
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