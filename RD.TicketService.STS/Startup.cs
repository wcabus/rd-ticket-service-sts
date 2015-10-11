using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Web.Helpers;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using RD.TicketService.Security;
using RD.TicketService.STS.Configuration;

[assembly:OwinStartup(typeof(RD.TicketService.STS.Startup))]
namespace RD.TicketService.STS
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/identity", idsrvApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });
                factory.ConfigureUserService();
                factory.ConfigureConsentService();

                var options = new IdentityServerOptions
                {
                    SiteName = "RealDolmen TicketService STS",
                    SigningCertificate = LoadCertificate(),
                    Factory = factory
                };

                idsrvApp.UseIdentityServer(options);
            });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = $"{SecurityConstants.IdentityServerUri}/identity",
                ClientId = "mvc",
                RedirectUri = $"{SecurityConstants.IdentityServerUri}/",
                ResponseType = "id_token",

                SignInAsAuthenticationType = "Cookies"
            });
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                $@"{AppDomain.CurrentDomain.BaseDirectory}\bin\Certificates\RD.TicketService.STS.pfx", "R34lD0lm3n");
        }
    }
}