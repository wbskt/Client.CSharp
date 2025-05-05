using System;

namespace Wbskt.Client.Contracts
{
    /// <summary>
    /// Represents the payload sent by the user to a Wbskt channel.
    /// This class encapsulates the data being transmitted to the channel.
    /// </summary>
    public class ClientPayload
    {
        /// <summary>
        /// Gets the data being sent to the Wbskt channel.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        public Guid PublisherId { get; set; }

        public bool EnsureDelivery { get; set; }

        public Guid ChannelSubscriberId { get; set; }

        public Guid PayloadId { get; set; }
    }
}
