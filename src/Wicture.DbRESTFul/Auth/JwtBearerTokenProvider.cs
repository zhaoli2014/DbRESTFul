using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Wicture.DbRESTFul.Infrastructure;

namespace Wicture.DbRESTFul.Auth
{
    public class JwtBearerTokenProvider
    {
        private static readonly TimeSpan defaultExpiration = TimeSpan.FromHours(4);
        private readonly TokenProviderOptions options;

        private readonly Schema[] schemas = new[]
        {
            new Schema { name = "username", nullable = false, type = "string" },
            new Schema { name = "password", nullable = false, type = "string" },
        };

        public JwtBearerTokenProvider(TimeSpan expiration, string secretKey = "Wicture@Micr0serv!ce^Secret", string audience = "Microservice", string issuer = "Wicture")
        {
            options = new TokenProviderOptions(secretKey, audience, issuer) { Expiration = expiration };
        }

        public JwtBearerTokenProvider(string secretKey = "Wicture@Micr0serv!ce^Secret", string audience = "Microservice", string issuer = "Wicture")
        {
            options = new TokenProviderOptions(secretKey, audience, issuer) { Expiration = defaultExpiration };
        }

        public TokenValidationParameters Setup(IApplicationBuilder app)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = options.SigningCredentials.Key,

                ValidateIssuer = false,
                ValidIssuer = options.Issuer,

                ValidateAudience = true,
                ValidAudience = options.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,
                AuthenticationType = JwtBearerDefaults.AuthenticationScheme
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters,
                AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme,
            });

            app.Use(TokenRequest);

            return tokenValidationParameters;
        }

        private async Task TokenRequest(HttpContext context, Func<Task> next)
        {
            if (!context.Request.Path.Equals(options.Path, StringComparison.Ordinal))
            {
                await next();
            }
            else if (!context.Request.Method.Equals("POST"))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Bad request.");
            }
            else
            {
                context.Response.ContentType = "application/json";
                try
                {
                    var data = RequestDataParser.Parse(null, schemas, context);

                    var authData = await AuthIndentifierManager.AuthIdentifier.GetAuthData(data.Value<string>("username"), data.Value<string>("password"));
                    if (authData == null && authData.Identity == null)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid username or password.");
                        return;
                    }

                    JObject token = FillToken(authData);

                    await context.Response.WriteAsync(token.ToString(Formatting.Indented));
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Authorization failed. {Environment.NewLine}{ex.Message}");
                }
            }
        }

        private JObject FillToken(AuthData authData)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                    new Claim(ClaimTypes.NameIdentifier, authData.Identity.Id ?? ""),
                    new Claim(ClaimTypes.Name, authData?.Identity.Name ?? ""),
                    new Claim(ClaimTypes.Role, authData?.Identity.Role ?? ""),
                    new Claim(ClaimTypes.GivenName, authData?.Identity.Alias ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, now.GetUnixTime().ToString(), ClaimValueTypes.Integer64)
            };

            var jwt = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(options.Expiration),
                signingCredentials: options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            JObject token = new JObject();
            token[".issued"] = now.GetUnixTime();
            token[".expires"] = now.Add(options.Expiration).GetUnixTime();
            token["id"] = authData.Identity.Id;
            token["name"] = authData.Identity.Name;
            token["role"] = authData.Identity.Role;
            token["alias"] = authData.Identity.Alias;
            token["expires_in"] = now.Add(options.Expiration).GetUnixTime();
            token["access_token"] = encodedJwt;
            token["token_type"] = "bearer";

            if (authData.Data != null)
            {
                foreach (var item in authData.Data)
                {
                    token[item.Key] = item.Value.ToString();
                }
            }

            return token;
        }
    }
}
