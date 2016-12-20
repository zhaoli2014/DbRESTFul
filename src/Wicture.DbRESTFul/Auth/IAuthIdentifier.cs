using System.Threading.Tasks;

namespace Wicture.DbRESTFul.Auth
{
    public interface IAuthIdentifier
    {
        Task<AuthData> GetAuthData(string username, string password);
    }
}
