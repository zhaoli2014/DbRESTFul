using System;

namespace Wicture.DbRESTFul
{
    public class ApiInvocationException : Exception
    {
        public ApiInvocationException()
        {
        }

        public ApiInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
