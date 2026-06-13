using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Marketplace;
using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Services;
using Puhu.Settings;
using Servus.Application.Startup;

namespace Puhu.Setup;

public sealed class PluginSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var sp = services.BuildServiceProvider();
        var ctx = sp.GetRequiredService<SetupContext>();
        var settings = sp.GetRequiredService<ISettingsStore>();

        IReadOnlyList<IPuhuPlugin> builtInPlugins =
        [
            new MarketplacePlugin(),
            new SettingsPlugin(),
        ];

        var pluginRegistry = PluginLoader.DiscoverAndConfigure(services, builtInPlugins);
        ctx.PluginRegistry = pluginRegistry;
        services.AddSingleton(pluginRegistry);

        var savedOrder = settings.Get<string[]>(TabOrderService.OrderKey) ?? [];
        var orderedTabs = TabOrder.Apply(savedOrder, pluginRegistry.PluginTabs);
        TabRegistry.RegisterTabs(orderedTabs);

        services.AddSingleton<ITabOrderService>(new TabOrderService(settings, orderedTabs));
    }
}
