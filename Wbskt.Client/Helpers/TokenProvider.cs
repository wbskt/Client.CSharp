using System;
using System.Net.Http;
#if NETSTANDARD
using System.Text;
using System.Text.Json;
#elif NET
using System.Net.Http.Json;
#endif
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wbskt.Client.Configurations;
using Wbskt.Client.Contracts;

namespace Wbskt.Client.Helpers
{
    /// <summary>
    /// Provides tokens for Wbskt authentication.
    /// </summary>
    internal static class TokenProvider
    {
        public static async Task<string> GetTokenAsync(IWbsktConfiguration configuration, ILogger logger)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{configuration.WbsktServerAddress}")
            };

            try
            {
                var clientConnReq = new ClientConnectionRequest
                {
                    ChannelSecret = configuration.ChannelDetails.Secret,
                    ClientName = configuration.ClientDetails.Name,
                    ClientUniqueId = configuration.ClientDetails.UniqueId,
                    ChannelSubscriberId = configuration.ChannelDetails.SubscriberId
                };

#if NETSTANDARD
                var json = JsonSerializer.Serialize(clientConnReq);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync("/api/channels/client", content);
#elif NET
                var result = await httpClient.PostAsync("/api/channels/client", JsonContent.Create(clientConnReq));
#endif

                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }

                logger?.LogError("Response from token request: {message}, statusCode: {code}", result.ReasonPhrase, result.StatusCode);
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Cannot reach server: {baseAddress}, error: {error}", httpClient.BaseAddress, ex.Message);
                throw;
            }
        }
    }
}
