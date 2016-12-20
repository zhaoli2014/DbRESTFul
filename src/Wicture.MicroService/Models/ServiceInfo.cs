using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using Wicture.DbRESTFul;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.MicroService.Models
{
    public class ServiceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string ArchiveUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid Guid { get; set; }
        public string Location { get; set; }
        public DateTime LoadedAt { get; set; }
        public DateTime StartedAt { get; set; }
    }

    public static class ServiceInfoHelper
    {
        private static Schema[] schemas;

        static ServiceInfoHelper()
        {
            schemas = new[]
            {
                new Schema {name = "id", type = "string", nullable = false},
                new Schema {name = "name", type = "string", nullable = false},
                new Schema {name = "version", type = "string", nullable = false},
                new Schema {name = "archiveUrl", type = "string", nullable = false},
            };
        }

        public static ServiceInfo ParseRequest(HttpContext context)
        {
            try
            {
                var data = RequestDataParser.Parse(null, schemas, context);
                return data.ToObject<ServiceInfo>();
            }
            catch (Exception ex)
            {
                throw new Exception("Parse ServiceModel failed.", ex);
            }
        }

        public static void Save(string path, ServiceInfo model)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(model, Formatting.Indented));
        }

        public static ServiceInfo Load(string path)
        {
            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ServiceInfo>(content);
        }
    }
}