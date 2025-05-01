using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Wbskt.Client
{
    internal static class Helper
    {
        public static HostString GetSocketServerAddress(this IEnumerable<Claim> claims)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "SocketServer");

            var addrString = claim?.Value.Split('|').Last();
            return new HostString(addrString);
        }

        public static Guid GetTokenId(this IEnumerable<Claim> claims)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "TokenId");

            return Guid.Parse(claim.Value);
        }
    }
}
