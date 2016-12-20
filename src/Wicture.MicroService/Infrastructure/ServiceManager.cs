using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wicture.DbRESTFul;
using Wicture.MicroService.Models;

namespace Wicture.MicroService
{
    public static class ServiceMiddleware
    {
        private static IConsulClient consulClient;
        private static ConsulConfig consulConfig;

        private static readonly IServiceResolver[] resolvers;
        private static ServiceBootstrap serviceBootstrap;

        static ServiceMiddleware()
        {
            resolvers = new[]
            {
                new DatabaseServiceResolver()
            };
        }


        /// <summary>
        /// 提供服务的注册和取消注册服务入口。
        /// service/register：注册服务，
        /// service/deregister：取消服务注册
        /// </summary>
        /// <param name="app"></param>
        public static void UseServiceRegistry(this IApplicationBuilder app, IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            consulConfig = app.ApplicationServices.GetRequiredService<IOptions<ConsulConfig>>().Value;
            var consulServerAddress = config["CONSUL_SERVER_ADDRESS"];
            if (!string.IsNullOrEmpty(consulServerAddress))
            {
                consulConfig.Address = consulServerAddress;
            }

            app.Route("register", RegisterService, "GET");
            app.Route("deregister", DeregisterService, "GET");

            foreach (var item in resolvers)
            {
                item.Resolve(consulClient, config);
            }
        }

        /// <summary>
        /// 提供服务容器管理。
        /// service/deregister：取消服务注册，
        /// service/info：查看当前服务信息，
        /// service/load：动态加载服务，
        /// service/restart：重启服务
        /// </summary>
        /// <param name="app"></param>
        public static void UseServiceContainer(this IApplicationBuilder app, IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            serviceBootstrap = new ServiceBootstrap();

            consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            consulConfig = app.ApplicationServices.GetRequiredService<IOptions<ConsulConfig>>().Value;
            var consulServerAddress = config["CONSUL_SERVER_ADDRESS"];
            if (!string.IsNullOrEmpty(consulServerAddress))
            {
                consulConfig.Address = consulServerAddress;
            }

            app.Route("deregister", DeregisterService, "GET");

            app.Route("info", ServiceInfo, "GET");
            app.Route("load", LoadService, "POST");
            app.Route("restart", RestartService, "GET");

            foreach (var item in resolvers)
            {
                item.Resolve(consulClient, config);
            }

            serviceBootstrap.Boot(consulClient, loggerFactory);
        }

        private static IApplicationBuilder Route(this IApplicationBuilder app, string actionName,
            Func<HttpContext, Task> action, params string[] methods)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals("/service/" + actionName))
                {
                    try
                    {
                        if (methods != null && methods.Length > 0 && !methods.Contains(context.Request.Method))
                        {
                            throw new Exception(
                                $"Rquest Method {context.Request.Method} is not supported, please use {string.Join(",", methods)}.");
                        }

                        await action(context);
                    }
                    catch (Exception ex)
                    {
                        await WriteErrorAsync(context, ex);
                    }
                }
                else
                {
                    await next();
                }
            });

            return app;
        }


        private static async Task DeregisterService(HttpContext context)
        {
            var all = context.Request.Query["all"];

            await consulClient.DeregisterMicroServiceAsync(!string.IsNullOrEmpty(all));
            await WriteResultAsync(context, new {success = true});
        }


        private static async Task RegisterService(HttpContext context)
        {
            var serviceEntrypoint = Environment.GetEnvironmentVariable("SERVICE_ENTRYPOINT");
            if (string.IsNullOrEmpty(serviceEntrypoint))
            {
                Environment.SetEnvironmentVariable("SERVICE_ENTRYPOINT", $"{context.Request.Scheme}://{context.Request.Host}");
            }

            var assembly = Assembly.GetEntryAssembly();
            await consulClient.RegisterMicroServiceAsync(new ServiceInfo
            {
                Id = assembly.ManifestModule.ModuleVersionId.ToString(),
                Name = Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name),
                Version = assembly.ImageRuntimeVersion
            }, false);
            await WriteResultAsync(context, new { success = true });
        }


        private static async Task RestartService(HttpContext context)
        {
            serviceBootstrap.Restart();
            await WriteResultAsync(context, new {restarted = true});
        }

        private static async Task ServiceInfo(HttpContext context)
        {
            await WriteResultAsync(context, new
            {
                serviceBootstrap.ServiceInfo,
                serviceBootstrap.AvaliableServices
            });
        }

        private static async Task LoadService(HttpContext context)
        {
            var model = ServiceInfoHelper.ParseRequest(context);

            await serviceBootstrap.LoadService(model);
            await WriteResultAsync(context, serviceBootstrap.ServiceInfo);
        }

        private static async Task WriteResultAsync(HttpContext context, object data)
        {
            context.Response.Headers["Content-Type"] = "application/json";
            await
                context.Response.WriteAsync(JsonConvert.SerializeObject(AjaxResult.SetResult(data), Formatting.Indented));
        }

        private static async Task WriteErrorAsync(HttpContext context, Exception ex)
        {
            context.Response.Headers["Content-Type"] = "application/json";
            await
                context.Response.WriteAsync(JsonConvert.SerializeObject(AjaxResult.SetError(ex.ToString(), "500"),
                    Formatting.Indented));
        }
    }
}