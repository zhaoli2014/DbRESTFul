using Microsoft.AspNetCore.Http;

namespace Wicture.DbRESTFul.Infrastructure
{
    public interface IResponseResultResolver
    {
        object Resolve(HttpContext context, string statusCode, string errorMessage, object data);
    }
}
