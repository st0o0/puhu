using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Servus.Plugin.Marketplace;
using Servus.Plugin;
using Servus.Plugin.Settings;
using Servus.TUI.Nodes;

namespace Servus.TUI.Setup;

public sealed class PluginSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();

        IReadOnlyList<IServusPlugin> builtInPlugins =
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
