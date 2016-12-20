using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wicture.DbRESTFul
{
    public static class MiddleWareFactory
    {
        public static Dictionary<string, IMiddleWare> MiddleWares { get; private set; }

        static MiddleWareFactory()
        {
            MiddleWares = new Dictionary<string, IMiddleWare>();
            var mwType = typeof(IMiddleWare);

            var types = Assembly.Load(new AssemblyName("Wicture.DbRESTFul")).GetTypes();
            foreach (var item in types.Where(t => t.GetTypeInfo().IsClass && mwType.IsAssignableFrom(t)))
            {
                var instance = Activator.CreateInstance(item) as IMiddleWare;
                MiddleWares.Add(instance.Name, instance);
            }
        }

        public static void Resolve(InvokeContext context, ActivePosition position)
        {
            if (context.CodeItem.middleWares == null)
            {
                return;
            }

            foreach (var item in context.CodeItem.middleWares)
            {
                if (MiddleWares.ContainsKey(item.Key))
                {
                    var middleWare = MiddleWares[item.Key];
                    if (middleWare.ActivePosition == position)
                    {
                        var message = middleWare.Validate(context, item.Value);
                        if (string.IsNullOrEmpty(message))
                        {
                            middleWare.Resolve(context, item.Value);
                        }
                        else
                        {
                            throw new Exception("Resolving middle ware '{0}' failed. {1}{2}".FormatWith(item.Key, Environment.NewLine, message));
                        }
                    }
                }
                else
                {
                    throw new Exception("Unknown middle ware '{0}'.".FormatWith(item.Key));
                }
            }
        }
    }
}