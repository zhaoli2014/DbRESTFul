using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wicture.DbRESTFul;

namespace Wicture.MicroService
{
    public class DatabaseServiceResolver : IServiceResolver
    {
        public async Task Resolve(IConsulClient consulClient, IConfigurationRoot config)
        {
            await Task.Run(() =>
            {
                try
                {
                    var services = consulClient.Agent.Services().Result.Response;
                    var readDbService = services.FirstOrDefault(s => s.Value.Service == "read-mysql");
                    var writeDbService = services.FirstOrDefault(s => s.Value.Service == "write-mysql");
                    var settings = GetConnectionSettings(ConfigurationManager.Settings.CSI.ReadDbConnectionString);

                    ConfigurationManager.Settings.CSI.ReadDbConnectionString
                        = ConfigurationManager.Settings.CSI.ReadDbConnectionString
                            .Replace(settings["server"], readDbService.Value.Address)
                            .Replace(settings["port"], readDbService.Value.Port.ToString());

                    if (!string.IsNullOrEmpty(config["READ_DB_USERNAME"]))
                        ConfigurationManager.Settings.CSI.ReadDbConnectionString =
                            ConfigurationManager.Settings.CSI.ReadDbConnectionString.Replace(settings["user"],
                                config["READ_DB_USERNAME"]);
                    if (!string.IsNullOrEmpty(config["READ_DB_PASSWORD"]))
                        ConfigurationManager.Settings.CSI.ReadDbConnectionString =
                            ConfigurationManager.Settings.CSI.ReadDbConnectionString.Replace(settings["password"],
                                config["READ_DB_PASSWORD"]);

                    ConfigurationManager.Settings.CSI.WriteDbConnectionString = ConfigurationManager.Settings
                        .CSI.WriteDbConnectionString
                        .Replace(settings["server"], writeDbService.Value.Address)
                        .Replace(settings["port"], writeDbService.Value.Port.ToString());

                    if (!string.IsNullOrEmpty(config["WRITE_DB_USERNAME"]))
                        ConfigurationManager.Settings.CSI.WriteDbConnectionString =
                            ConfigurationManager.Settings.CSI.WriteDbConnectionString.Replace(settings["user"],
                                config["WRITE_DB_USERNAME"]);
                    if (!string.IsNullOrEmpty(config["WRITE_DB_PASSWORD"]))
                        ConfigurationManager.Settings.CSI.WriteDbConnectionString =
                            ConfigurationManager.Settings.CSI.WriteDbConnectionString.Replace(settings["password"],
                                config["WRITE_DB_PASSWORD"]);
                }
                catch (Exception ex)
                {
                    LogHelper.Warn("Resolve Database connection failed, use default connection string instead.", ex);
                }
            });
        }

        private Dictionary<string, string> GetConnectionSettings(string conn)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in conn.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = item.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 2)
                {
                    result.Add(kv[0], kv[1]);
                }
            }

            return result;
        }
    }
}