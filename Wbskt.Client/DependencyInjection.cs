using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wbskt.Client.Configurations;

namespace Wbskt.Client
{
    public static class DependencyInjection
    {
        public static void ConfigureWbsktListener(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IWbsktConfiguration, WbsktConfiguration>();
            serviceCollection.Configure<WbsktConfiguration.Settings>(configuration);
            serviceCollection.AddSingleton<WbsktListener>();
        }
    }
}
