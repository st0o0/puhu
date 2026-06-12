using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Puhu.Plugin.Marketplace;
using Puhu.Plugin.Settings;
using Puhu.Plugin;
using Puhu.TUI.Nodes;

namespace Puhu.TUI.Setup;

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
