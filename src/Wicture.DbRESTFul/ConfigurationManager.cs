using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul
{
    public static class ConfigurationManager
    {
        private static object reloadLocker = new object();
        public static Settings Settings { get; set; }
        private static ILogger logger;
        private static bool isDevelopment;

        public static event EventHandler OnLoaded;

        public static void Setup(string configFilePath, ILoggerFactory loggerFactory, bool isDevelopment = false)
        {
            logger = loggerFactory?.CreateLogger("ConfigurationManager");
            ConfigurationManager.isDevelopment = isDevelopment;

            if (!File.Exists(configFilePath))
            {
                var ex = new ArgumentException($"The config file not exists in path: {configFilePath}.");
                logger?.LogCritical(new EventId(), ex, "Reload config file failed.");
                throw ex;
            }

            WatchConfigFile(configFilePath);

            Load(configFilePath);
        }

        private static void Load(string configFilePath)
        {
            var content = File.ReadAllText(configFilePath);
            Settings = JsonConvert.DeserializeObject<Settings>(content);

            var dir = Path.GetDirectoryName(configFilePath);

            Settings.API.Path = Path.Combine(dir, Settings.API.Path);
            Settings.CSI.Path = Path.Combine(dir, Settings.CSI.Path);
            Settings.CRI.Path = Path.Combine(dir, Settings.CRI.Path);

            Settings.Repository.Path = isDevelopment 
                ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                : Path.Combine(dir, Settings.Repository.Path);

            if (!Directory.Exists(Settings.API.Path)) Directory.CreateDirectory(Settings.API.Path);
            if (!Directory.Exists(Settings.CSI.Path)) Directory.CreateDirectory(Settings.CSI.Path);
            if (!Directory.Exists(Settings.CRI.Path)) Directory.CreateDirectory(Settings.CRI.Path);
            if (!Directory.Exists(Settings.Repository.Path)) Directory.CreateDirectory(Settings.Repository.Path);

            ServiceResourceManager.Load();

            OnLoaded?.Invoke(null, null);
        }

        private static void WatchConfigFile(string path)
        {
            FileSystemWatcher watchFolder = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            try
            {
                watchFolder.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Security;
                watchFolder.EnableRaisingEvents = true;
                DateTime lastRead = DateTime.MinValue;

                watchFolder.Changed += (s, e) =>
                {
                    watchFolder.EnableRaisingEvents = false;
                    lock (reloadLocker)
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(path);
                        if (lastWriteTime != lastRead)
                        {
                            lastRead = lastWriteTime;
                            Load(path);
                        }
                    }
                    watchFolder.EnableRaisingEvents = true;
                };
            }
            catch(Exception ex)
            {
                logger?.LogError(new EventId(), ex, "Reload config file failed.");
            }
        }
    }

    public class Settings
    {
        public APISetting API { get; set; }
        public CSISetting CSI { get; set; }
        public CRISetting CRI { get; set; }
        public RepositorySetting Repository { get; set; }
        public DevConfig DevConfig { get; set; }
        public Dictionary<string, string> Variables { get; set; }
    }

    public class APISetting
    {
        public bool ShowFullException { get; set; }
        public string Path { get; set; }
        public string Profix { get; set; }
        public CacheConfig[] CacheConfigs { get; set; }
    }

    public class CSISetting
    {
        public DatabaseType DatabaseType { get; set; }
        public string Path { get; set; }
        public string ReadDbConnectionString { get; set; }
        public string WriteDbConnectionString { get; set; }
        public int CommandTimeout { get; set; }
    }

    public class CRISetting
    {
        public bool ApiOnly { get; set; }
        public string Path { get; set; }
        public string UnwrapParameterName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class RepositorySetting
    {
        public string Path { get; set; }
        public string[] Includes { get; set; }
        public string[] Excludes { get; set; }
    }

    public class CacheConfig
    {
        public string Type { get; set; }
        public JObject Config { get; set; }
    }

    public class DevConfig
    {
        public DatabaseType DatabaseType { get; set; }
        public string ProjectName { get; set; }
        public string DevDbConnectionString { get; set; }
    }

    public enum DatabaseType
    {
        MySQL,
        SQLServer
    }
}
