using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Wicture.DbRESTFul.Cache;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul.Infrastructure
{
    public class ConfiguredApiFilter
    {
        public IApiAuthorizeFilter ApiAuthorizeFilter { get; set; }
        
        public IResponseResultResolver ResponseResultResolver { get; set; }

        public bool Execute(HttpContext context)
        {
            var abPath = context.Request.Path.Value;
            ConfiguredAPI api;
            if (!ServiceResourceManager.TryMapRoute(abPath, out api))
            {
                LoggerManager.Logger.LogDebug($"No API is found for request `{abPath}`.");
                return false;
            }
            LoggerManager.Logger.LogDebug($"API {api.name} is found to for the requst `{abPath}`.");

            if (context.Request.Method != api.method)
            {
                var message = string.Format("HTTP Method '{0}' is not support, expected: '{1}'", context.Request.Method, api.method);
                CompositeResponse(context, JsonConvert.SerializeObject(new { statusCode = 405, errorMessage = message }), 405);
            }
            else
            {
                ApiInvokingArgs invokingArgs = null;
                IdentityInfo identity = null;

                try
                {
                    var param = RequestDataParser.Parse(api.parameter.query, api.parameter.body, context);
                    invokingArgs = new ApiInvokingArgs(context, api, param);
                    ApiInvocationHandler.OnInvoking(invokingArgs);

                    var result = ExcuteApi(api, context, param);
                    CompositeResponse(context, JsonConvert.SerializeObject(result));
                    identity = context.Items.ContainsKey("identity") ? context.Items["identity"] as IdentityInfo : null;

                    ApiInvocationHandler.OnInvoked(new ApiInvokedArgs(invokingArgs, identity, result));
                }
                catch (LogicalException ex)
                {
                    var result = ResponseResultResolver.Resolve(context, ex.ErrorCode, ex.Message, null);
                    CompositeResponse(context, JsonConvert.SerializeObject(result));
                    identity = context.Items.ContainsKey("identity") ? context.Items["identity"] as IdentityInfo : null;

                    var parameters = invokingArgs?.Param != null ? JsonConvert.SerializeObject(invokingArgs?.Param, Formatting.Indented) : "";
                    ApiInvocationHandler.OnInvoked(new ApiInvokedArgs(invokingArgs, identity, result));
                }
                catch (Exception ex)
                {
                    var exception = ex is TargetInvocationException ? ex.InnerException : ex;

                    var message = ConfigurationManager.Settings.API.ShowFullException ? exception.ToString() : "服务出错。";
                    var result = ResponseResultResolver.Resolve(context, "500", message, null);
                    CompositeResponse(context, JsonConvert.SerializeObject(result));

                    var parameters = invokingArgs?.Param != null ? JsonConvert.SerializeObject(invokingArgs?.Param, Formatting.Indented) : "";
                    LoggerManager.Logger.LogError(exception, $"Execute request `{abPath}` failed, data: {Environment.NewLine}{parameters}. {Environment.NewLine}{exception}");
                    identity = context.Items.ContainsKey("identity") ? context.Items["identity"] as IdentityInfo : null;
                    ApiInvocationHandler.OnError(new ApiExceptionArgs(invokingArgs, identity, exception));
                }
            }

            return true;
        }

        public Task<bool> ExecuteAsync(HttpContext context)
        {
            return Task.Run(() => Execute(context));
        }

        private object ExcuteApi(ConfiguredAPI api, HttpContext context, JToken param)
        {
            if (!api.implemented)
            {
                if (api.mock == null)
                {
                    throw new Exception("No mock data configured.");
                }

                var data = ResolveMock(param, api.mock);
                return data;
            }

            var identity = ApiAuthorizeFilter.OnAuthorization(context, api.allowAnonymous);

            context.Items.Add("identity", identity);

            var cacheKey = context.Request.Path + (context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "");
            if (api.cache?.enabled == true)
            {
                var data = CacheProviderFactory.Get(api.cache.type, cacheKey);

                if (data != null)
                {
                    return SetStandardResult(context, data);
                }
            }

            object result;
            if (api.implementation.type == "csi")
            {
                DbRESTFulRepository repository = new DbRESTFulRepository() { HttpContext = context, CurrentUser = identity };
                result = repository.Invoke(api.implementation.name, param);
            }
            else if (api.implementation.type == "repository")
            {
                result = RepositoryInvoker.Call(api.implementation.name, param, identity, context);
            }
            else if (api.implementation.type == "cri")
            {
                result = RedisInvoker.Call(api.implementation.name, param);
            }
            else
            {
                throw new Exception(string.Format("Unsupported implementation type '{0}'", api.implementation.type));
            }

            if (api.cache?.enabled == true)
            {
                CacheProviderFactory.Set(api.cache.type, cacheKey, result, api.cache.expiration);
            }

            return SetStandardResult(context, result);
        }

        private object SetStandardResult(HttpContext context, object result)
        {
            return ResponseResultResolver.Resolve(context, "200", string.Empty, result);
        }

        private object ResolveMock(JToken param, ApiMock[] mock)
        {
            foreach (var item in mock)
            {
                var matched = true;
                foreach (var token in item.input)
                {
                    if (token.Value.Value<string>() != "*" && token.Value.Value<string>() != param[token.Key].Value<string>())
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return item.output;
                }
            }

            throw new Exception("Unexpected parameters, no mock data matches.");
        }

        private void CompositeResponse(HttpContext context, string content, int statusCode = 200)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            context.Response.StatusCode = statusCode;
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.Body.Write(buffer, 0, buffer.Length);

            LoggerManager.Logger.LogDebug($"Composited result for request `{context.Request.Path}`.");
        }
    }
}
