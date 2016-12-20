using Consul;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Wicture.MicroService
{
    public interface IServiceResolver
    {
        Task Resolve(IConsulClient consulClient, IConfigurationRoot config);
    }
}