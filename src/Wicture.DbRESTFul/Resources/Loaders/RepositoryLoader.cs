using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Wicture.DbRESTFul.MiddleWares;

namespace Wicture.DbRESTFul.Resources
{
    public class RepositoryLoader : IResourceLoader<RepositoryModel>
    {
        public Dictionary<string, RepositoryModel> Map { get; private set; } = new Dictionary<string, RepositoryModel>();

        public string ResourcePath { get; set; }

        public List<Assembly> Assemblies { get; private set; } = new List<Assembly>();

        public void Load(bool silently)
        {
            Map.Clear();

            LoadAssemblies();
            var baseType = typeof(DbRESTFulRepository);
            foreach (var assembly in Assemblies)
            {
                assembly.GetTypes()
                    .Where(t => baseType.IsAssignableFrom(t) && t.Name != baseType.Name && !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsInterface)
                    .ForEach(t => LoadMethods(t));
            }

            PreprocessMiddleWare.InitPreprocess(Assemblies);
        }

        private void LoadAssemblies()
        {
            var includes = new Func<string[], string, bool>((source, file) =>
            {
                if (source == null && source.Length == 0) return true;
                return source.Select(f => Path.GetFileNameWithoutExtension(f).ToLower()).Contains(file.ToLower());
            });

            var dllfiles = Directory.GetFiles(ConfigurationManager.Settings.Repository.Path, "*.dll")
                .Where(n => 
                {
                    var fn = Path.GetFileNameWithoutExtension(n);
                    return !fn.Equals(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location))
                            && !fn.StartsWith("Microsoft.")
                            && !fn.StartsWith("System.")
                            && !fn.Equals("Wicture.DbRESTFul", StringComparison.CurrentCultureIgnoreCase)
                            && includes(ConfigurationManager.Settings.Repository.Includes, fn)
                            && !includes(ConfigurationManager.Settings.Repository.Excludes, fn);
                });

            Assemblies.Clear();

            Assemblies.AddRange(dllfiles.Select(file => AssemblyLoadContext.Default.LoadFromAssemblyPath(file)));
            Assemblies.Add(Assembly.GetEntryAssembly());
        }

        private void LoadMethods(Type repositoryType)
        {
            try
            {
                foreach (var item in repositoryType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    Map.Add($"{repositoryType.Name}.{item.Name}", new RepositoryModel(repositoryType, item));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Create instance for Repository type {repositoryType} failed, please make sure it has default constructor.", ex);
            }
        }
    }
}
