using System;

namespace Wbskt.Client.Contracts
{
    /// <summary>
    /// Represents the payload sent by the user to a Wbskt channel.
    /// This class encapsulates the data being transmitted to the channel.
    /// </summary>
    public class UserClientPayload
    {
        /// <summary>
        /// Gets the data being sent to the Wbskt channel.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Gets the publisherId to which the payload was sent to.
        /// </summary>
        public Guid PublisherId { get; set; }

        /// <summary>
        /// Gets the channelId of the channel from which the payload was received from. (info: multiple channels can be triggered using a single publisherId)
        /// </summary>
        public Guid ChannelSubscriberId { get; set; }

        /// <summary>
        /// Gets the uniqueId for the payload sent by the user.
        /// </summary>
        public Guid PayloadId { get; set; }
    }
}
