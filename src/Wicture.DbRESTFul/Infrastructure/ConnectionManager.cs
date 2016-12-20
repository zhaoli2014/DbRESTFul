using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Wicture.DbRESTFul
{
    public class ConnectionManager
    {
        public string ReadConnectionString { get; set; }

        public string WriteConnectionString { get; set; }

        public int? CommandTimeout = null;

        public ConnectionManager()
        {
            CommandTimeout = ConfigurationManager.Settings.CSI.CommandTimeout;
        }

        private void EnsureConnectionStrings()
        {
            if (string.IsNullOrEmpty(ReadConnectionString))
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.ReadDbConnectionString) 
                    && !string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.WriteDbConnectionString))
                {
                    ReadConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
                    WriteConnectionString = ConfigurationManager.Settings.CSI.WriteDbConnectionString;
                }
                else if (!string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.ReadDbConnectionString))
                {
                    ReadConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
                    WriteConnectionString = ConfigurationManager.Settings.CSI.ReadDbConnectionString;
                }
                else
                {
                    throw new Exception("No 'ReadDb' and 'WriteDb' connection strings in config file.");
                }
            }
        }

        public ConnectionManager(string readConnectionString, string writeConnectionString = null)
        {
            if (string.IsNullOrEmpty(readConnectionString))
            {
                throw new ArgumentNullException(readConnectionString);
            }

            ReadConnectionString = readConnectionString;
            WriteConnectionString = string.IsNullOrEmpty(writeConnectionString) ? readConnectionString : writeConnectionString;
        }

        public void UseGateway(Dictionary<object, object> bag)
        {
            if (bag.ContainsKey("ReadConnectionString")) ReadConnectionString = bag["ReadConnectionString"].ToString();
            if (bag.ContainsKey("WriteConnectionString")) WriteConnectionString = bag["WriteConnectionString"].ToString();
        }

        public IDbConnection ReadConnection
        {
            get
            {
                EnsureConnectionStrings();

                return ConfigurationManager.Settings.CSI.DatabaseType == DatabaseType.MySQL 
                    ? new MySqlConnection(ReadConnectionString) as IDbConnection
                    : new SqlConnection(ReadConnectionString) as IDbConnection;
            }
        }

        public IDbConnection WriteConnection
        {
            get
            {
                EnsureConnectionStrings();

                return ConfigurationManager.Settings.CSI.DatabaseType == DatabaseType.MySQL
                    ? new MySqlConnection(WriteConnectionString) as IDbConnection
                    : new SqlConnection(WriteConnectionString) as IDbConnection;
            }
        }
    }
}