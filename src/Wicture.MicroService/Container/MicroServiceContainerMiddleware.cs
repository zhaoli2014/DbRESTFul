using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wicture.DbRESTFul;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.MicroService.Middlewares
{
    public static class MicroServiceContainerMiddleware
    {
        public static IApplicationBuilder UseMicroServiceContainer(this IApplicationBuilder app, IConfigurationRoot configuration,
            IApiAuthorizeFilter authorizeFilter)
        {
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbRESTFulAPI");
            app.UseServiceContainer(configuration, loggerFactory);

            LoggerManager.Use(logger);
            var apiFilter = new ConfiguredApiFilter
            {
                ApiAuthorizeFilter = authorizeFilter,
                ResponseResultResolver = new DefaultResponseResultResolver()
            };

            app.UseMiddleware<DbRESTFulServiceMiddleware>(apiFilter);

            return app;
        }
    }
}