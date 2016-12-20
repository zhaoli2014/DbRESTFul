using System.Threading.Tasks;

namespace Wicture.DbRESTFul.Auth
{
    public class DefaultAuthIdentifier : IAuthIdentifier
    {
        public Task<AuthData> GetAuthData(string username, string password)
        {
            return Task.FromResult(new AuthData { Identity = new IdentityInfo { Id = "0", Name = "Wicture" } });
        }
    }
}
