using Wicture.DbRESTFul;

namespace Wicture.MicroService.Models
{
    public class MemberInfo : IdentityInfo
    {
        public string Token { get; set; }
    }
}