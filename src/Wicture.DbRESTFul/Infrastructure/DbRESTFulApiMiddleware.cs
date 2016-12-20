using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Wicture.DbRESTFul.Cache;

namespace Wicture.DbRESTFul.Infrastructure
{
    public class DbRESTFulApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ConfiguredApiFilter filter;
        private readonly bool apiOnly;

        private readonly static DateTime startedTime = DateTime.Now;
        private readonly static Guid guid = Guid.NewGuid();

        public DbRESTFulApiMiddleware(RequestDelegate next, ConfiguredApiFilter filter, bool apiOnly)
        {
            this.next = next;
            this.filter = filter;
            this.apiOnly = apiOnly;
        }

        public async Task Invoke(HttpContext context)
        {
            if (apiOnly && context.Request.Path.Value.Equals("/"))
            {
                context.Response.Headers["Content-Type"] = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    application = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().ManifestModule.Name),
                    guid,
                    requestUrl = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.QueryString}",
                    status = "Running",
                    startedAt = startedTime,
                    serverTime = DateTime.Now
                }, Formatting.Indented));
            }
            else if(!await filter.ExecuteAsync(context))
            {
                await next.Invoke(context);
            }
        }
    }

    #region ExtensionMethod

    public static class UserDbRESTFulMiddlewareExtension
    {
        public static IApplicationBuilder UserDbRESTFul(this IApplicationBuilder app, 
            IApiAuthorizeFilter authorizeFilter,
            bool apiOnly = false,
            IResponseResultResolver responseResultResolver = null)
        {
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbRESTFulAPI");
            LoggerManager.Use(logger);

            var apiFilter = new ConfiguredApiFilter() { ApiAuthorizeFilter = authorizeFilter };

            CacheProviderFactory.Init();

            apiFilter.ResponseResultResolver = responseResultResolver == null 
                ? new DefaultResponseResultResolver()
                : responseResultResolver;

            app.UseMiddleware<DbRESTFulApiMiddleware>(apiFilter, apiOnly);

            return app;
        }
    }

    #endregion
}
