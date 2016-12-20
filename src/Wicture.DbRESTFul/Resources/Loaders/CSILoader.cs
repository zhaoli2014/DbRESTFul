using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Wicture.DbRESTFul.Converters;

namespace Wicture.DbRESTFul.Resources
{
    public class CSILoader : IResourceLoader<CSIItem>
    {
        public Dictionary<string, CSIItem> Map { get; private set; } = new Dictionary<string, CSIItem>();

        public string ResourcePath { get; set; }

        private const string csiFileExtension = "*.*";

        public void Load(bool silently)
        {
            Map.Clear();

            if (string.IsNullOrEmpty(ConfigurationManager.Settings.CSI.Path))
            {
                if (silently) return;
                throw new Exception("CSI path is not specified");
            }
            if (!Directory.Exists(ConfigurationManager.Settings.CSI.Path))
            {
                if (silently) return;
                throw new Exception("CSI path not Exists");
            }

            string[] files = Directory.GetFiles(ConfigurationManager.Settings.CSI.Path, csiFileExtension);

            string currentFile = string.Empty;
            try
            {
                foreach (var file in files)
                {
                    currentFile = file;
                    string jsonData = File.ReadAllText(file);
                    var items = JsonConvert.DeserializeObject<List<CSIItem>>(jsonData);

                    foreach (var item in items)
                    {
                        if (Map.ContainsKey(item.name))
                            throw new CSINameExistsException(string.Format("CSI name {0} already exists. ", item.name));

                        Map.Add(item.name, item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Parsing csi file:'{0}' failed.", currentFile), ex);
            }
        }
    }
}
