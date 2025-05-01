using System;

namespace Wbskt.Client.Configurations
{
    public class ChannelDetails
    {
        /// <summary>
        /// The id of the channel this client will be configured to listen
        /// </summary>
        public Guid SubscriberId { get; set; }

        /// <summary>
        /// This is a secret string provided by the user while creation of a channel
        /// </summary>
        public string Secret { get; set; } = string.Empty;
    }
}
