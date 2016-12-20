using System.Threading.Tasks;
using Wicture.DbRESTFul;
using Wicture.DbRESTFul.Auth;

namespace Wicture.MicroService.Auth
{
    public class DefaultAuthIdentifier : IAuthIdentifier
    {
        public Task<AuthData> GetAuthData(string username, string password)
        {
            return Task.FromResult(new AuthData { Identity = new IdentityInfo { Id = "0" } });
        }
    }
}
