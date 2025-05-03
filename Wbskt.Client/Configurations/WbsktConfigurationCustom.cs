namespace Wbskt.Client.Configurations
{
    /// <summary>
    /// We recommend using appsettings.json or IConfiguration to configure  <see cref="WbsktListener"/> thourgh the <see cref="WbsktConfiguration"/>.
    /// You can also use custom configuration with this class. See documentation for detailed explanation.
    /// </summary>
    public class WbsktConfigurationCustom : IWbsktConfiguration
    {
        /// <summary>
        /// Gets the Wbskt server address for the WebSocket client.
        /// </summary>
        public string WbsktServerAddress { get; set; }

        /// <summary>
        /// Gets the client-specific details such as name, unique ID, and retry interval.
        /// </summary>
        public ClientDetails ClientDetails { get; set; }

        /// <summary>
        /// Gets the channel-specific details such as subscriber ID and secret.
        /// </summary>
        public ChannelDetails ChannelDetails { get; set; }
    }
}
