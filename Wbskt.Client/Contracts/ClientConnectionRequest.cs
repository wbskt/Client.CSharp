using System;
using System.ComponentModel.DataAnnotations;

namespace Wbskt.Client.Contracts
{
    internal class ClientConnectionRequest
    {
        /// <summary>
        /// Details of all the channels the client is subscribed to
        /// </summary>
        public ClientChannel[] Channels { get; set; } = new ClientChannel[] { };

        /// <summary>
        /// Human-readable name for the client
        /// </summary>
        [Required]
        public string ClientName { get; set; }

        /// <summary>
        /// This id is unique for a particular client. This is an identifier for this specific client throughout the platform
        /// </summary>
        [Required]
        public Guid ClientUniqueId { get; set;}
    }

    public class ClientChannel
    {
        /// <summary>
        /// Used for a client to connect to a channel
        /// </summary>
        public Guid ChannelSubscriberId { get; set; }

        /// <summary>
        /// This is a secret string provided by the user while creation of a channel
        /// </summary>
        public string ChannelSecret { get; set; }
    }
}
