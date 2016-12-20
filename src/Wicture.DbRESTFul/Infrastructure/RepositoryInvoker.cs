using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul
{
    public static class RepositoryInvoker
    {
        public static object Call(string name, JToken param, IdentityInfo userInfo, HttpContext context)
        {
            if (!ServiceResourceManager.Actions.ContainsKey(name))
            {
                throw new Exception("The repository implementation '{0}' is not found.".FormatWith(name));
            }

            var item = ServiceResourceManager.Actions[name];
            var instance = Activator.CreateInstance(item.Type) as DbRESTFulRepository;
            instance.HttpContext = context;
            instance.CurrentUser = userInfo;

            try
            {
                return item.Method.Invoke(instance, item.Method.GetParameters().Length == 0 ? null : new object[] { param });
            }
            catch(TargetInvocationException ex)
            {
                // 因为通过反射调用时，所有方法抛出的异常会被包在TargetInvocationException的InnerException里。
                if (ex.InnerException != null && ex.InnerException is LogicalException)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
