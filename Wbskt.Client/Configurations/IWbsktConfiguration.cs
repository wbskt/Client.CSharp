using Microsoft.AspNetCore.Http;

namespace Wbskt.Client.Configurations
{
    /// <summary>
    /// Represents the configuration settings required for the Wbskt client.
    /// This interface defines the properties needed to establish and manage a Wbskt connection.
    /// </summary>
    public interface IWbsktConfiguration
    {
        /// <summary>
        /// Gets the Wbskt server address for the Wbskt client.
        /// This is the base address of the Wbskt server.
        /// </summary>
        HostString WbsktServerAddress { get; }

        /// <summary>
        /// Gets the client-specific details, such as name, unique ID, and retry interval.
        /// These details are used to identify the client and manage reconnection attempts.
        /// </summary>
        ClientDetails ClientDetails { get; }

        /// <summary>
        /// Gets the channel-specific details, such as subscriber ID and secret.
        /// These details are used to authenticate the client with the Wbskt server.
        /// </summary>
        ChannelDetails ChannelDetails { get; }
    }
}
