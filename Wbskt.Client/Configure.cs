using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wbskt.Client
{
    public static class Configure
    {
        public static void ConfigureWbskt(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<WbsktConfiguration.Settings>(configuration);
            serviceCollection.AddSingleton<IWbsktConfiguration, WbsktConfiguration>();
            serviceCollection.AddSingleton<WbsktListener>();
        }
    }
}
