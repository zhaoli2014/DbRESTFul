using Microsoft.AspNetCore.Http;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.MicroService.Infrastructure
{
    public class WochuResponseResultResolver : IResponseResultResolver
    {
        public object Resolve(HttpContext context, string statusCode, string errorMessage, object data)
        {
            if (context.Request.Path.ToString().ToLowerInvariant().Equals("/token") && data != null)
            {
                return data;
            }

            return
                new
                {
                    hasError = !string.IsNullOrEmpty(errorMessage),
                    errorCode = statusCode,
                    statusCode,
                    errorMessage,
                    data
                };
        }
    }
}