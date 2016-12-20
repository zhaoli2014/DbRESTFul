using System.Collections.Generic;

namespace Wicture.DbRESTFul
{
    public class CodeModule
    {
        public string Name { get; set; }
        public List<CSIItem> Items { get; set; }

        public CodeModule()
        {
            Items = new List<CSIItem>();
        }
    }

    public class ConfiguredCode
    {
        public List<CodeModule> Modules { get; set; }

        public ConfiguredCode()
        {
            Modules = new List<CodeModule>();
        }
    }
}