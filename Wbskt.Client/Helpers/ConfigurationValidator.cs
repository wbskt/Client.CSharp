using Microsoft.Extensions.Logging;
using Wbskt.Client.Configurations;

namespace Wbskt.Client.Helpers
{
    /// <summary>
    /// Validates the WebSocket configuration.
    /// </summary>
    internal static class ConfigurationValidator
    {
        public static bool IsValid(IWbsktConfiguration configuration, ILogger logger)
        {
            if (configuration == null)
            {
                logger?.LogError("Configuration is not set.");
                return false;
            }

            if (configuration.ChannelDetails.SubscriberId == default)
            {
                logger?.LogError("SubscriberId is not set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(configuration.ChannelDetails.Secret))
            {
                logger?.LogError("Secret is not set.");
                return false;
            }

            if (configuration.ClientDetails.UniqueId == default)
            {
                logger?.LogError("UniqueId is not set.");
                return false;
            }

            return true;
        }
    }
}
