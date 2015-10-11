using System.Collections.Generic;
using System.Security.Claims;

namespace RD.TicketService.Security
{
    public class ClaimComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim x, Claim y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null || x.Type != y.Type)
                return false;
            return x.Value == y.Value;
        }

        public int GetHashCode(Claim claim)
        {
            if (ReferenceEquals(claim, null))
            {
                return 0;
            }

            return (claim.Type?.GetHashCode() ?? 0) ^ (claim.Value?.GetHashCode() ?? 0);
        }
    }
}