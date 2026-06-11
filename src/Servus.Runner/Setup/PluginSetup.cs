using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Servus.Plugin.Marketplace;
using Servus.Plugin.Sdk;
using Servus.Plugin.Settings;
using Servus.Runner.Nodes;

namespace Servus.Runner.Setup;

public sealed class PluginSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var tickSource = GetRegisteredSingleton<ITickSource>(services)
            ?? throw new InvalidOperationException(
                "ITickSource must be registered before PluginSetup. Ensure ServicesSetup runs first.");

        IReadOnlyList<IServusPlugin> builtInPlugins =
        [
            new MarketplacePlugin(),
            new SettingsPlugin(),
        ];

        var pluginRegistry = PluginLoader.DiscoverAndConfigure(services, tickSource, builtInPlugins);
        services.AddSingleton(pluginRegistry);
        TabBarNode.RegisterPluginTabs(pluginRegistry);
    }

    private static T? GetRegisteredSingleton<T>(IServiceCollection services) where T : class
    {
        var descriptor = services.LastOrDefault(d =>
            d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

        return descriptor?.ImplementationInstance as T;
    }
}
