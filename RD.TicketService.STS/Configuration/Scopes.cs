using System.Collections.Generic;
using IdentityServer3.Core.Models;
using RD.TicketService.Security;

namespace RD.TicketService.STS.Configuration
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.EmailAlwaysInclude,
                StandardScopes.Profile,
                new Scope
                {
                    Enabled = true,
                    Name = SecurityConstants.ApiScope,
                    Description = "Access to the API for RealDolmen TicketService",
                    Type = ScopeType.Resource,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("role")
                    }
                }
            };
        }
    }
}
