using System.Collections.Generic;

namespace Wicture.DbRESTFul
{
    public class IdentityInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Alias { get; set; }

        public Dictionary<object, object> GatewayBag { get; private set; }

        public IdentityInfo()
        {
            GatewayBag = new Dictionary<object, object>();
        }
    }
}
