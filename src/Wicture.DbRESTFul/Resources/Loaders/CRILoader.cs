using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Wicture.DbRESTFul.Converters;

namespace Wicture.DbRESTFul.Resources
{
    public class CRILoader : IResourceLoader<CRIItem>
    {
        public Dictionary<string, CRIItem> Map { get; private set; } = new Dictionary<string, CRIItem>();

        public string ResourcePath { get; set; }

        private const string rciFileExtension = "*.*";

        public void Load(bool silently)
        {
            Map.Clear();

            if (string.IsNullOrEmpty(ConfigurationManager.Settings.CRI.Path))
            {
                if (silently) return;
                throw new Exception("CRI path is not specified");
            }

            if (!Directory.Exists(ConfigurationManager.Settings.CRI.Path))
            {
                if (silently) return;
                throw new Exception("CRI path not Exists");
            }

            string[] files = Directory.GetFiles(ConfigurationManager.Settings.CRI.Path, rciFileExtension);

            string currentFile = string.Empty;
            try
            {
                foreach (var file in files)
                {
                    currentFile = file;
                    string jsonData = File.ReadAllText(file);
                    var items = JsonConvert.DeserializeObject<List<CRIItem>>(jsonData);

                    foreach (var item in items)
                    {
                        if (Map.ContainsKey(item.name))
                            throw new CRINameExistsException(string.Format("CRI name {0} already exists. ", item.name));

                        Map.Add(item.name, item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Parsing rci file:'{0}' failed.", currentFile), ex);
            }
        }
    }
}
