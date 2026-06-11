using Servus.Plugin.Marketplace.Pages;
using Servus.TUI.Plugin;

namespace Servus.Plugin.Marketplace;

public sealed class MarketplacePlugin : IServusPlugin
{
    public string Name => "Marketplace";

    public void Configure(IServusPluginBuilder builder)
    {
        builder
            .WithTab("Marketplace", "/marketplace")
            .ConfigureRoutes(ctx =>
                ctx.RegisterRoute<MarketplacePage, MarketplaceViewModel>("/marketplace"));
    }
}
