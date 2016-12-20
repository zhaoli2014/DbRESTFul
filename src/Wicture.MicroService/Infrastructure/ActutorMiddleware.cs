﻿using Dotnet.Microservice;
using Dotnet.Microservice.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Wicture.MicroService
{
    public static class ActutorMiddleware
    {
        public static void UseHealthEndpoint(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals("/health"))
                {
                    // Perform IP access check
                    if (MicroserviceConfiguration.AllowedIpAddresses != null
                        && context.Request.HttpContext.Connection.RemoteIpAddress != null
                        &&
                        !MicroserviceConfiguration.AllowedIpAddresses.Contains(
                            context.Request.HttpContext.Connection.RemoteIpAddress))
                    {
                        context.Response.StatusCode = 403;
                        await next();
                    }

                    var status = HealthCheckRegistry.GetStatus();

                    if (!status.IsHealthy)
                    {
                        // Return a service unavailable status code if any of the checks fail
                        context.Response.StatusCode = 503;
                    }

                    context.Response.Headers["Content-Type"] = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(status));
                }
                else
                {
                    await next();
                }
            });
        }

        public static void UseEnvironmentEndpoint(this IApplicationBuilder app, bool includeEnvVars = true)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals("/env"))
                {
                    // Perform IP access check
                    if (MicroserviceConfiguration.AllowedIpAddresses != null
                        && context.Request.HttpContext.Connection.RemoteIpAddress != null
                        &&
                        !MicroserviceConfiguration.AllowedIpAddresses.Contains(
                            context.Request.HttpContext.Connection.RemoteIpAddress))
                    {
                        context.Response.StatusCode = 403;
                        await next();
                    }

                    // Get current application environment
                    ApplicationEnvironment env = ApplicationEnvironment.GetApplicationEnvironment(includeEnvVars);

                    context.Response.Headers["Content-Type"] = "application/json";
                    await
                        context.Response.WriteAsync(JsonConvert.SerializeObject(env, Formatting.Indented,
                            new JsonSerializerSettings() {StringEscapeHandling = StringEscapeHandling.EscapeNonAscii}));
                }
                else
                {
                    await next();
                }
            });
        }

        public static void UseInfoEndpoint(this IApplicationBuilder app)
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            UseInfoEndpoint(app, entryAssembly.GetName());
        }

        private static DateTime startedTime = DateTime.Now;
        private static Guid appStamp = Guid.NewGuid();

        // .NET core does not contain the GetExecutingAssembly() method so we must pass in a reference to the entry assembly
        public static void UseInfoEndpoint(this IApplicationBuilder app, AssemblyName entryAssembly)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals("/info"))
                {
                    // Perform IP access check
                    if (MicroserviceConfiguration.AllowedIpAddresses != null
                        && context.Request.HttpContext.Connection.RemoteIpAddress != null
                        &&
                        !MicroserviceConfiguration.AllowedIpAddresses.Contains(
                            context.Request.HttpContext.Connection.RemoteIpAddress))
                    {
                        context.Response.StatusCode = 403;
                        await next();
                    }

                    var appInfo = new
                    {
                        Name = entryAssembly.Name,
                        Version = entryAssembly.Version.ToString(3),
                        stamp = appStamp,
                        requestUrl = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.QueryString}",
                        startedAt = startedTime,
                        serverTime = DateTime.Now
                    };

                    context.Response.Headers["Content-Type"] = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(appInfo, Formatting.Indented));
                }
                else
                {
                    await next();
                }
            });
        }
    }
}