using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.MicroService.Middlewares
{
    public class DbRESTFulServiceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ConfiguredApiFilter filter;

        private readonly static DateTime startedTime = DateTime.Now;
        private readonly static Guid guid = Guid.NewGuid();

        public DbRESTFulServiceMiddleware(RequestDelegate next, ConfiguredApiFilter filter)
        {
            this.next = next;
            this.filter = filter;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.Equals("/"))
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
            else if (!await filter.ExecuteAsync(context))
            {
                await next.Invoke(context);
            }
        }
    }
}