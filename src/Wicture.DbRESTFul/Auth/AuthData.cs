using System.Collections.Generic;

namespace Wicture.DbRESTFul.Auth
{
    public class AuthData
    {
        public Dictionary<string, object> Data { get; set; }
        public IdentityInfo Identity { get; set; }
    }
}
