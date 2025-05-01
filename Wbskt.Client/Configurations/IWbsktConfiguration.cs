using Microsoft.AspNetCore.Http;

namespace Wbskt.Client.Configurations
{
    public interface IWbsktConfiguration
    {
        HostString CoreServerAddress { get; }
        ClientDetails ClientDetails { get; }
        ChannelDetails ChannelDetails { get; }
    }
}
