using System.Collections.Generic;
using System.IdentityModel.Tokens;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using RD.TicketService.Security;

[assembly: OwinStartup(typeof(RD.TicketService.Api.Startup))]
namespace RD.TicketService.Api
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = $"{SecurityConstants.IdentityServerUri}/identity",
                RequiredScopes = new[] { SecurityConstants.ApiScope },
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            });

            var config = WebApiConfig.Register();
            app.UseWebApi(config);
        }
    }
}