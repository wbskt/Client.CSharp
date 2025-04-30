using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Wbskt.Client
{
    public class ChannelDetails
    {
        public Guid SubscriberId { get; set; }
        public string Secret { get; set; } = string.Empty;
    }

    public class ClientDetails
    {
        public string Name { get; set; } = $"{Environment.MachineName}:{Guid.NewGuid()}";
        public Guid UniqueId { get; set; }
        public int RetryIntervalInSeconds { get; set; } = 10;
    }

    public class WbsktConfiguration
    {
        private readonly IOptionsMonitor<Settings> _settingsMonitor;

        public WbsktConfiguration(IOptionsMonitor<Settings> settingsMonitor)
        {
            _settingsMonitor = settingsMonitor;
        }

        public HostString CoreServerAddress => new HostString(_settingsMonitor.CurrentValue.CoreServerAddress);
        public ClientDetails ClientDetails => _settingsMonitor.CurrentValue.ClientDetails;
        public ChannelDetails ChannelDetails => _settingsMonitor.CurrentValue.ChannelDetails;

        public class Settings
        {
            public string CoreServerAddress { get; set; } = string.Empty;
            public ClientDetails ClientDetails { get; set; } = new ClientDetails();
            public ChannelDetails ChannelDetails { get; set; } = new ChannelDetails();
        }
    }
}
