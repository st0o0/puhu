using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Plugin.Marketplace;
using Puhu.Plugin.Settings;
using Servus.Application.Startup;

namespace Puhu.Setup;

public sealed class PluginSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();

        IReadOnlyList<IPuhuPlugin> builtInPlugins =
        [
            new MarketplacePlugin(),
            new SettingsPlugin(),
        ];

        var pluginRegistry = PluginLoader.DiscoverAndConfigure(services, ctx.TickSource, builtInPlugins);
        ctx.PluginRegistry = pluginRegistry;
        services.AddSingleton(pluginRegistry);
        TabBarNode.RegisterPluginTabs(pluginRegistry);
    }
}
