using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Wicture.DbRESTFul.Auth
{
    public class TokenProviderOptions
    {
        public string Path { get; private set; } = "/token";

        public string Issuer { get; private set; }

        public string Audience { get; private set; }

        public TimeSpan Expiration { get; set; }

        public SigningCredentials SigningCredentials { get; private set; }

        public TokenProviderOptions(string secretKey, string audience, string issuer)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            Audience = audience;
            Issuer = issuer;
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        }
    }
}
