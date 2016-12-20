using Microsoft.AspNetCore.Http;
using Wicture.DbRESTFul.Gateway;

namespace Wicture.DbRESTFul.Infrastructure
{
    public interface IApiAuthorizeFilter
    {
        IdentityInfo OnAuthorization(HttpContext context, bool allowAnonymous);

        IDbGateway DbGateway { get; set; }

        void GatewayDetermination(HttpContext context, IdentityInfo identityInfo);
    }
}
