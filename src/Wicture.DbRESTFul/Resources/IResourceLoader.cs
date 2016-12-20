using System.Collections.Generic;

namespace Wicture.DbRESTFul.Resources
{
    public interface IResourceLoader<T>
    {
        void Load(bool silently);
        string ResourcePath { get; set; }
        Dictionary<string, T> Map { get; }
    }
}
