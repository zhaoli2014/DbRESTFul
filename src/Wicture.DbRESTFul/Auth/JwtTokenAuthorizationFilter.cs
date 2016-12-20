using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Wicture.DbRESTFul.Gateway;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.DbRESTFul.Auth
{
    public class JwtTokenAuthorizationFilter : IApiAuthorizeFilter
    {
        private readonly TokenValidationParameters tokenParameters;
        private readonly JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        public bool ReturnDefaultUserIfFault { get; set; } = false;

        public IDbGateway DbGateway { get; set; }

        public JwtTokenAuthorizationFilter(TokenValidationParameters tokenParameters)
        {
            this.tokenParameters = tokenParameters;
        }

        public virtual IdentityInfo OnAuthorization(HttpContext context, bool allowAnonymous)
        {
            var token = context.Request.Headers["Authorization"].ToString();
            var startIndex = "bearer ".Length;

            if (string.IsNullOrEmpty(token) || token.Length < startIndex || token.Substring(startIndex).Equals("null"))
            {
                if (ReturnDefaultUserIfFault || allowAnonymous)
                {
                    var identity = new IdentityInfo { Id = "0" };
                    GatewayDetermination(context, identity);
                    return identity;
                }

                throw new LogicalException("Unauthorized. The Authorization Token in Headers of Request is required.", "499");
            }

            try
            {
                token = token.Substring(startIndex);

                SecurityToken secToken;
                var pricipal = handler.ValidateToken(token, tokenParameters, out secToken);

                var userIdclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var userNameclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                var roleclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                var aliasclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);

                var result = new IdentityInfo()
                {
                    Id = userIdclaim == null || string.IsNullOrEmpty(userIdclaim.Value) ? "0" : userIdclaim.Value,
                    Name = userNameclaim == null || string.IsNullOrEmpty(userNameclaim.Value) ? "" : userNameclaim.Value,
                    Role = roleclaim == null || string.IsNullOrEmpty(roleclaim.Value) ? "" : roleclaim.Value,
                    Alias = aliasclaim?.Value ?? ""
                };

                GatewayDetermination(context, result);
                return result;
            }
            catch (Exception)
            {
                throw new LogicalException("Unauthorized, Token validation failed.", "401");
            }
        }

        public void GatewayDetermination(HttpContext context, IdentityInfo identityInfo)
        {
            if (DbGateway == null) return;

            var conn = DbGateway.Process(context, identityInfo);

            identityInfo.GatewayBag.Add("ReadConnectionString", conn.ReadConnectionString);
            identityInfo.GatewayBag.Add("WriteConnectionString", conn.WriteConnectionString);
        }
    }
}
