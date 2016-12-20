using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul
{
    public class Schema
    {
        public string name { get; set; }
        public string type { get; set; }
        public bool nullable { get; set; }
        public string description { get; set; }

        public Schema[] schema { get; set; }
    }

    public class ApiParameter
    {
        public Schema[] query { get; set; }
        public Schema[] body { get; set; }
    }

    public class ApiImplementation
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class ApiMataData
    {
        public string type { get; set; }
        public Schema[] schema { get; set; }
    }

    public class ApiMock
    {
        public JObject input { get; set; }
        public JToken output { get; set; }
    }

    public class ApiCache
    {
        public bool enabled { get; set; }
        public string type { get; set; }
        public int expiration { get; set; }
    }

    public class ConfiguredAPI
    {
        public string version { get; set; }
        public string owner { get; set; }
        public string updatedTime { get; set; }
        public string name { get; set; }
        public string module { get; set; }
        public string url { get; set; }
        public string method { get; set; }
        public string title { get; set; }
        public string summary { get; set; }
        public string note { get; set; }
        public bool allowAnonymous { get; set; } = true;
        public bool useAbsoluteUrl { get; set; } = false;
        public ApiCache cache { get; set; }
        public bool implemented { get; set; }
        public ApiImplementation implementation { get; set; }
        public ApiParameter parameter { get; set; }
        public ApiMataData result { get; set; }
        public ApiMock[] mock { get; set; }
    }
}
