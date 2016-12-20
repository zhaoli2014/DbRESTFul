using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul.Controllers
{
    [Route("dbrestful/api/ca")]
    public class CAController : ApiControllerBase<DbRESTFulRepository>
    {
        private static readonly List<string> ObjectProperties = new List<string> { "cache", "implementation", "parameter", "result", "mock" };

        private IHostingEnvironment env;

        public CAController(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment) 
            : base(loggerFactory)
        {
            env = hostingEnvironment;
        }

        [HttpGet]
        [Route("sync")]
        public object SyncAllConfiguredAPIs(bool groupWithModule = false)
        {
            if (!env.IsDevelopment())
            {
                return Error("API Sync is disabled due to it is not in Dev stage.", "500");
            }

            var sql = "SELECT a.version, a.owner, from_unixtime(a.updatedTime) as updatedTime,a.name,a.module,a.url,a.useAbsoluteUrl,a.method,a.title,a.summary,a.note,a.allowAnonymous,a.cache,a.implemented,a.implementation,a.parameter,a.result,a.mock FROM api AS a LEFT JOIN project AS p ON p.id = a.projectId WHERE p.name = @projectName";

            using (MySqlConnection conn = new MySqlConnection(ConfigurationManager.Settings.DevConfig.DevDbConnectionString))
            {
                var param = new { projectName = ConfigurationManager.Settings.DevConfig.ProjectName };
                IEnumerable<dynamic> result = repository.InvokeCodeWithConnection(conn, sql, param);
                if (groupWithModule)
                {
                    foreach (IEnumerable<dynamic> items in result.GroupBy(i => i.module))
                    {
                        var module = items.FirstOrDefault().module;
                        var path = string.Format("{0}/{1}.json", ConfigurationManager.Settings.API.Path, module);
                        SaveAPIsToFile(path, items.ToArray());
                    }
                }
                else
                {
                    foreach (var item in result)
                    {
                        var path = string.Format("API/{0}/{1}.json", item.module, item.name);
                        SaveAPIsToFile(path, item);
                    }
                }

                ServiceResourceManager.APILoader.Load(false);
                return Result(new { count = result.Count() });
            }
        }

        private static void SaveAPIsToFile(string path, params dynamic[] rawApis)
        {
            var apis = new JArray();
            foreach (var item in rawApis)
            {
                JObject ca = JObject.FromObject(item);
                ca["implemented"] = ca.Value<bool?>("implemented") ?? false;
                ca["useAbsoluteUrl"] = ca.Value<bool?>("useAbsoluteUrl") ?? false;
                ca["allowAnonymous"] = ca.Value<bool?>("allowAnonymous") ?? false;
                ObjectProperties.ForEach(p => ca[p] = JToken.Parse(ca.Value<string>(p) ?? "{}"));
                apis.Add(ca);
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            System.IO.File.WriteAllText(path, apis.ToString(Formatting.Indented));
        }

        [HttpGet]
        [Route("generateSpec")]
        public object GenerateSpec(string module)
        {
            return Result(new { success = true, path = module });
        }
    }
}
