using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Wicture.DbRESTFul;
using Wicture.DbRESTFul.Cache;
using Wicture.DbRESTFul.Resources;
using Wicture.MicroService.Models;
using Consul;
using Wicture.DbRESTFul.Microservice;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Wicture.MicroService
{
    public class ServiceBootstrap
    {
        private readonly string serviceDir;
        private readonly string serviceInfoFilePath;

        public List<ServiceInfo> AvaliableServices { get; private set; }

        private ServiceInfo serviceInfo;

        public ServiceInfo ServiceInfo
        {
            get { return serviceInfo; }
            set
            {
                if (serviceInfo != value)
                {
                    serviceInfo = value;
                    ServiceInfoHelper.Save(serviceInfoFilePath, serviceInfo);
                }
            }
        }

        public ServiceBootstrap()
        {
            AvaliableServices = new List<ServiceInfo>();

            serviceDir = Path.Combine(Directory.GetCurrentDirectory(), "service");
            if (!Directory.Exists(serviceDir)) Directory.CreateDirectory(serviceDir);
            serviceInfoFilePath = Path.Combine(serviceDir, "serviceInfo.json");
        }

        public void Boot(IConsulClient consulClient, ILoggerFactory logerFactory)
        {
            foreach (var dir in Directory.GetDirectories(serviceDir))
            {
                var serviceFilePath = Path.Combine(dir, "serviceInfo.json");
                if (!File.Exists(serviceFilePath)) continue;

                AvaliableServices.Add(ServiceInfoHelper.Load(serviceFilePath));
            }

            if (File.Exists(serviceInfoFilePath))
            {
                serviceInfo = ServiceInfoHelper.Load(serviceInfoFilePath);
            }
            else if (AvaliableServices.Count > 0)
            {
                ServiceInfo = AvaliableServices.OrderByDescending(s => s.LoadedAt).First();
            }

            Start(consulClient, logerFactory);
        }

        private void Start(IConsulClient consulClient, ILoggerFactory logerFactory)
        {
            if (ServiceInfo != null)
            {
                var configFilePath = Path.Combine(ServiceInfo.Location, "config.json");
                ConfigurationManager.Setup(configFilePath, logerFactory);
                ServiceResourceManager.Load();
                CacheProviderFactory.Init();

                var lifetime = GetMicroserviceLifetime();
                lifetime?.Start();

                ServiceInfo.StartedAt = DateTime.Now;
            }

            consulClient.RegisterMicroServiceAsync(ServiceInfo);
        }

        public void Restart()
        {
            if (Environment.GetEnvironmentVariable("OS")?.Contains("Windows") == true)
            {
                Process.Start("./restart.bat", Directory.GetCurrentDirectory());
            }
            else
            {
                Process.Start("./restart.sh");
            }
        }

        public async Task LoadService(ServiceInfo model)
        {
            try
            {
                if (ServiceInfo?.Id == model.Id) return;

                var service = AvaliableServices.FirstOrDefault(s => s.Id == model.Id);
                if (service != null)
                {
                    ServiceInfo = service;
                }
                else
                {
                    var path = Path.Combine(serviceDir,
                        $"{model.Name}-v{model.Version}{Path.GetExtension(model.ArchiveUrl)}");
                    using (HttpClient client = new HttpClient())
                    {
                        await DownloadFile(client, model.ArchiveUrl, path);
                    }

                    var targetDir = Path.Combine(serviceDir, Path.GetFileNameWithoutExtension(path));
                    Unzip(path, targetDir);
                    File.Delete(path);

                    model.Guid = Guid.NewGuid();
                    model.LoadedAt = DateTime.Now;
                    model.Location = targetDir;
                    ServiceInfoHelper.Save(Path.Combine(targetDir, "serviceInfo.json"), model);

                    ServiceInfo = model;
                }

                Restart();
            }
            catch (Exception ex)
            {
                throw new Exception("Load service failed.", ex);
            }
        }

        private async Task DownloadFile(HttpClient client, string downloadUrl, string targetFilePath)
        {
            using (
                HttpResponseMessage response =
                    client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                response.EnsureSuccessStatusCode();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (
                    var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                        8192, true))
                {
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads += 1;

                            if (totalReads%2000 == 0)
                            {
                                Console.WriteLine(string.Format("total bytes downloaded so far: {0:n0}", totalRead));
                            }
                        }
                    } while (isMoreToRead);
                }
            }
        }

        private void Unzip(string filePath, string targetDir)
        {
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            using (Stream stream = File.OpenRead(filePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(targetDir, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        private MicroserviceLifetime GetMicroserviceLifetime()
        {
            var type = typeof(MicroserviceLifetime);
            foreach (var assembly in ServiceResourceManager.RepositoryLoader.Assemblies)
            {
                var mlType = assembly.GetTypes().FirstOrDefault(t => type.IsAssignableFrom(t) && t.Name != type.Name && !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsInterface);
                if (mlType != null)
                {
                    return Activator.CreateInstance(mlType) as MicroserviceLifetime;
                }
            }

            return null;
        }
    }
}