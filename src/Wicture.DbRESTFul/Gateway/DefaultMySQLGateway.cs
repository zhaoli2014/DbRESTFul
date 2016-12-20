using Microsoft.AspNetCore.Http;
using System;

namespace Wicture.DbRESTFul.Gateway
{
    /// <summary>
    /// 默认的读写分离数据库网关
    /// </summary>
    public class DefaultMySQLGateway : IDbGateway
    {
        public DatabaseConnection Process(HttpContext context, IdentityInfo userInfo)
        {
            var result = new DatabaseConnection();

            if (!string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.ReadDbConnectionString) 
                && !string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.WriteDbConnectionString))
            {
                result.ReadConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
                result.WriteConnectionString = ConfigurationManager.Settings.CSI.WriteDbConnectionString;
            }
            else if (!string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.ReadDbConnectionString))
            {
                result.ReadConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
                result.WriteConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
            }
            else
            {
                throw new Exception("No 'ReadDb' and 'WriteDb' connection strings in config file.");
            }

            return result;
        }
    }
}
