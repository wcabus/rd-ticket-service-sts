using System.Collections.Generic;
using IdentityServer3.Core.Models;
using RD.TicketService.Security;

namespace RD.TicketService.STS.Configuration
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "RD TicketService Web client",
                    ClientId = SecurityConstants.AngularClientId,
                    Flow = Flows.Implicit,
                    AllowAccessToAllScopes = true,
                    RedirectUris = new List<string>
                    {
                        "http://localhost:9000/callback.html"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:9000/"
                    }
                },

                // Test client (STS itself)
                new Client
                {
                    Enabled = true,
                    ClientName = "STS test",
                    ClientId = "mvc",
                    Flow = Flows.Implicit,
                    AllowAccessToAllScopes = true,
                    RedirectUris = new List<string>
                    {
                        "https://localhost:44301/"
                    }
                }
            };
        }
    }
}