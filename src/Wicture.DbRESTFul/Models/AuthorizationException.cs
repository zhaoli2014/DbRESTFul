using System;

namespace Wicture.DbRESTFul.Models
{
    public class AuthorizationException : Exception
    {
        public AuthorizationException(string message) 
            : base(message)
        {
        }
    }
}
