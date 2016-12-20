using Microsoft.AspNetCore.Http;

namespace Wicture.DbRESTFul.Infrastructure
{
    public class DefaultResponseResultResolver : IResponseResultResolver
    {
        public object Resolve(HttpContext context, string statusCode, string errorMessage, object data)
        {
            return new { statusCode, errorMessage, data };
        }
    }
}
