using System.Collections.Generic;
using System.IO;

namespace Wicture.DbRESTFul.Resources
{
    public static class ServiceResourceManager
    {
        public static Dictionary<string, ConfiguredAPI> Apis { get { return APILoader.Map; } }
        public static Dictionary<string, CSIItem> CSIs { get { return CSILoader.Map; } }
        public static Dictionary<string, CRIItem> CRIs { get { return CRILoader.Map; } }
        public static Dictionary<string, RepositoryModel> Actions { get { return RepositoryLoader.Map; } }

        public static bool UrlCaseInsensitive { get; set; } = true;
        
        public static CSILoader CSILoader { get; private set; }
        public static CRILoader CRILoader { get; private set; }
        public static APILoader APILoader { get; private set; }
        public static RepositoryLoader RepositoryLoader { get; private set; }

        static ServiceResourceManager()
        {
            CSILoader = new CSILoader();
            CRILoader = new CRILoader();
            APILoader = new APILoader();
            RepositoryLoader = new RepositoryLoader();
        }

        public static void ClearUp()
        {
            Actions.Clear();

            if (Directory.Exists(ConfigurationManager.Settings?.CSI.Path)) Directory.Delete(ConfigurationManager.Settings.CSI.Path, true);
            if (Directory.Exists(ConfigurationManager.Settings?.API.Path)) Directory.Delete(ConfigurationManager.Settings.API.Path, true);
            if (Directory.Exists(ConfigurationManager.Settings?.CRI.Path)) Directory.Delete(ConfigurationManager.Settings.CRI.Path, true);
            if (Directory.Exists(ConfigurationManager.Settings?.Repository.Path)) Directory.Delete(ConfigurationManager.Settings.Repository.Path, true);
        }

        public static void Load(bool silently = false)
        {
            CSILoader.Load(silently);
            CRILoader.Load(silently);
            APILoader.Load(silently);
            RepositoryLoader.Load(silently);
        }

        public static bool TryMapRoute(string path, out ConfiguredAPI api)
        {
            var key = UrlCaseInsensitive ? path.ToLowerInvariant() : path;

            bool matched = APILoader.Map.ContainsKey(key);
            api = matched ? APILoader.Map[key] : null;

            return matched;
        }
    }
}
