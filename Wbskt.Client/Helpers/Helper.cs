using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Wbskt.Client.Helpers
{
    internal static class Helper
    {
        public static string GetSocketServerAddress(this IEnumerable<Claim> claims)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "SocketServer");

            var addrString = claim?.Value.Split('|').Last();
            return addrString;
        }

        public static Guid GetTokenId(this IEnumerable<Claim> claims)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "TokenId");

            return Guid.Parse(claim?.Value ?? Guid.Empty.ToString());
        }
    }
}
