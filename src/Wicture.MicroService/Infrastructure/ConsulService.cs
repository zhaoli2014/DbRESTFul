using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wicture.DbRESTFul.Resources;
using Wicture.MicroService.Infrastructure;
using Wicture.MicroService.Models;

namespace Wicture.MicroService
{
    public static class ConsulService
    {
        private static string MicroServiceName = "Wicture.MicroService";

        public static async Task<IEnumerable<AgentService>> QueryService(this IConsulClient client)
        {
            var result = await client.Agent.Services();
            return result.Response.Values;
        }

        public static async Task RegisterMicroServiceAsync(this IConsulClient client, ServiceInfo info, bool containerized = true)
        {
            var serviceEntrypoint = Environment.GetEnvironmentVariable("SERVICE_ENTRYPOINT");
            if (!string.IsNullOrEmpty(serviceEntrypoint))
            {
                var uri = new Uri(serviceEntrypoint);
                var apiTags = RoutesTable.Routes.Count > 0
                    ? RoutesTable.Routes.Select(api => $"urlprefix-{api}").ToList()
                    : ServiceResourceManager.Apis?.Select(api => $"urlprefix-{api.Key}").ToList();

                if (info != null)
                {
                    apiTags?.Insert(0, $"SERVICE:{info.Id}@{info.Name}-{info.Version}");
                }

                MicroServiceName = containerized ? MicroServiceName : info.Name;

                var registration = new AgentServiceRegistration()
                {
                    ID = $"{MicroServiceName}@{uri.Host}:{uri.Port}",
                    Name = MicroServiceName,
                    Address = $"{uri.Host}",
                    Port = uri.Port,
                    Tags = apiTags?.ToArray(),
                    Check = new AgentServiceCheck()
                    {
                        Timeout = TimeSpan.FromSeconds(3),
                        Interval = TimeSpan.FromSeconds(10),
                        HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}/health"
                    }
                };

                await client.Agent.ServiceDeregister(registration.ID)
                    .Then(() => client.Agent.ServiceRegister(registration));
            }
        }

        public static async Task DeregisterMicroServiceAsync(this IConsulClient client, bool all)
        {
            if (all)
            {
                var result = await client.QueryService();
                if (result != null)
                {
                    var services = result.Where(s => s.Service == MicroServiceName).Select(s => s.ID);
                    services.ForEach(async id => await client.Agent.ServiceDeregister(id));
                }
            }
            else
            {
                var serviceEntrypoint = Environment.GetEnvironmentVariable("SERVICE_ENTRYPOINT");
                if (!string.IsNullOrEmpty(serviceEntrypoint))
                {
                    var uri = new Uri(serviceEntrypoint);
                    var serviceId = $"{MicroServiceName}@{uri.Host}:{uri.Port}";
                    await client.Agent.ServiceDeregister(serviceId);
                }
            }
        }

        // Adapted from: http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx
        public static Task Then(this Task first, Func<Task> next)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (next == null) throw new ArgumentNullException(nameof(next));

            var tcs = new TaskCompletionSource<object>();
            first.ContinueWith(t1 =>
            {
                if (first.IsFaulted) tcs.TrySetException(first.Exception?.InnerExceptions);
                else if (first.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var nextTask = next();
                        if (nextTask == null) tcs.TrySetCanceled();
                        else
                            nextTask.ContinueWith(t2 =>
                            {
                                if (nextTask.IsFaulted) tcs.TrySetException(nextTask.Exception.InnerExceptions);
                                else if (nextTask.IsCanceled) tcs.TrySetCanceled();
                                else tcs.TrySetResult(null);
                            }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}