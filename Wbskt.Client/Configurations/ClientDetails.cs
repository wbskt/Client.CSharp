using System;

namespace Wbskt.Client.Configurations
{
    public class ClientDetails
    {
        /// <summary>
        /// Human-readable name for the client (may not be unique)
        /// </summary>
        public string Name { get; set; } = $"{Environment.MachineName}:{Guid.NewGuid()}";

        /// <summary>
        /// This id should unique for a particular client. Server will allow only one connection per `UniqueId`
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// Time in seconds this client should wait before retrying for a new connection. (in case of connection failure)
        /// </summary>
        public int RetryIntervalInSeconds { get; set; } = 10;
    }
}
