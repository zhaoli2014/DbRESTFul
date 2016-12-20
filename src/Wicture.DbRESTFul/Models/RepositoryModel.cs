using System;
using System.Reflection;

namespace Wicture.DbRESTFul
{
    public class RepositoryModel
    {
        public RepositoryModel()
        {

        }

        public RepositoryModel(Type type, MethodInfo method)
        {
            Type = type;
            Method = method;
        }

        public Type Type { get; set; }
        public MethodInfo Method { get; set; }
    }
}
