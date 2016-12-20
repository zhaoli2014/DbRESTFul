using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Wicture.DbRESTFul.Resources
{
    public class APILoader : IResourceLoader<ConfiguredAPI>
    {
        private const string APIProfix = "/api/";
        private const string apiFileExtention = "*.*";

        public Dictionary<string, ConfiguredAPI> Map { get; private set; } = new Dictionary<string, ConfiguredAPI>();

        public string ResourcePath { get; set; }

        private string profix;

        public void Load(bool silently)
        {
            Map.Clear();
            profix = string.IsNullOrEmpty(ConfigurationManager.Settings.API.Profix) ? APIProfix : ConfigurationManager.Settings.API.Profix;

            if (string.IsNullOrEmpty(ConfigurationManager.Settings.API.Path))
            {
                if (silently) return;
                throw new Exception("API path is not specified");
            }

            if (!Directory.Exists(ConfigurationManager.Settings.API.Path))
            {
                if (silently) return;
                throw new Exception("API path not Exists");
            }

            LoadFile(ConfigurationManager.Settings.API.Path);
        }

        private void LoadFile(string dir)
        {
            string lastFile = string.Empty;
            try
            {
                if (!Directory.Exists(dir)) return;

                foreach (var file in Directory.GetFiles(dir, apiFileExtention))
                {
                    lastFile = file;
                    var text = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<List<ConfiguredAPI>>(text);
                    foreach (var item in data)
                    {
                        var url = item.useAbsoluteUrl ? item.url : (profix + item.module + "/" + item.url);
                        var key = ServiceResourceManager.UrlCaseInsensitive ? url.ToLowerInvariant() : url;
                        if (Map.ContainsKey(key))
                            throw new Exception(string.Format($"The url {key} already exists for API {item.name} in file '{file}'"));

                        Map.Add(key, item);
                    }
                }

                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    LoadFile(subDir);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Load Api failed, file: " + lastFile, ex);
            }
        }
    }
}
