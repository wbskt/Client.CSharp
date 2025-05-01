using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Wbskt.Client.Configurations
{
    /// <summary>
    /// This class provides configuration settings for the WebSocket client.
    /// It uses <see cref="IOptionsMonitor{TOptions}"/> to support hot-reload of configuration values. Please configure <see cref="DependencyInjection"/> to use this.
    /// </summary>
    internal sealed class WbsktConfiguration : IWbsktConfiguration
    {
        private readonly IOptionsMonitor<Settings> _settingsMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WbsktConfiguration"/> class.
        /// </summary>
        /// <param name="settingsMonitor">Monitors configuration settings for changes.</param>
        internal WbsktConfiguration(IOptionsMonitor<Settings> settingsMonitor)
        {
            _settingsMonitor = settingsMonitor;
        }

        /// <summary>
        /// Gets the core server address for the WebSocket client.
        /// </summary>
        public HostString CoreServerAddress => new HostString(_settingsMonitor.CurrentValue.CoreServerAddress);

        /// <summary>
        /// Gets the client-specific details such as name, unique ID, and retry interval.
        /// </summary>
        public ClientDetails ClientDetails => _settingsMonitor.CurrentValue.ClientDetails;

        /// <summary>
        /// Gets the channel-specific details such as subscriber ID and secret.
        /// </summary>
        public ChannelDetails ChannelDetails => _settingsMonitor.CurrentValue.ChannelDetails;

        /// <summary>
        /// Represents the configuration settings for the WebSocket client.
        /// </summary>
        internal class Settings
        {
            /// <summary>
            /// The address of the core server.
            /// </summary>
            public string CoreServerAddress { get; set; } = string.Empty;

            /// <summary>
            /// Details about the client, including name, unique ID, and retry interval.
            /// </summary>
            public ClientDetails ClientDetails { get; set; } = new ClientDetails();

            /// <summary>
            /// Details about the channel, including subscriber ID and secret.
            /// </summary>
            public ChannelDetails ChannelDetails { get; set; } = new ChannelDetails();
        }
    }
}
