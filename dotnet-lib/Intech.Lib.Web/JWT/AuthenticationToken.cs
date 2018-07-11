#region Usings
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal; 
#endregion

namespace Intech.Lib.Web.JWT
{
    public static class AuthenticationToken
    {
        /// <summary>
        /// Cria um token com informações básicas do usuario
        /// </summary>
        /// <param name="signingConfigurations"></param>
        /// <param name="tokenConfigurations"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static JsonWebToken Generate(SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations, string username)
            => Generate(signingConfigurations, tokenConfigurations, CreateIdentity(username));

        /// <summary>
        /// Cria um token com claims específicos
        /// </summary>
        /// <param name="signingConfigurations"></param>
        /// <param name="tokenConfigurations"></param>
        /// <param name="username"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static JsonWebToken Generate(SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations, string username, List<KeyValuePair<string, string>> claims)
        {
            ClaimsIdentity identity = CreateIdentity(username);

            foreach (var claim in claims)
                identity.AddClaim(new Claim(claim.Key, claim.Value));

            return Generate(signingConfigurations, tokenConfigurations, identity);
        }

        /// <summary>
        /// Cria um token recebendo um identity já criado
        /// </summary>
        /// <param name="signingConfigurations"></param>
        /// <param name="tokenConfigurations"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static JsonWebToken Generate(SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations, ClaimsIdentity identity)
        {
            DateTime dataCriacao = DateTime.Now;
            DateTime dataExpiracao = dataCriacao + TimeSpan.FromDays(500);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = tokenConfigurations.Issuer,
                Audience = tokenConfigurations.Audience,
                SigningCredentials = signingConfigurations.SigningCredentials,
                Subject = identity,
                NotBefore = dataCriacao,
                Expires = dataExpiracao,
            });
            var token = handler.WriteToken(securityToken);

            return new JsonWebToken
            {
                Authenticated = true,
                Created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                Expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                AccessToken = token,
                Message = "OK"
            };
        }

        public static JwtSecurityToken Decode(string token) 
            => new JwtSecurityTokenHandler().ReadJwtToken(token);

        private static ClaimsIdentity CreateIdentity(string username)
        {
            return new ClaimsIdentity(
                new GenericIdentity(username, "Login"),
                new[] {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.UniqueName, username)
                }
            );
        }
    }
}

namespace System
{
    public static class ClaimsExtensions
    {
        public static string GetValue(this IEnumerable<Claim> claims, string type)
            => claims.ToList().SingleOrDefault(x => x.Type == type).Value;
    }
}
