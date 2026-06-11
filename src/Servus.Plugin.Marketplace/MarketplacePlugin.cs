using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Servus.Plugin.Marketplace.Actors;
using Servus.Plugin.Marketplace.Models;
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
            .WithServices(services => services.AddSingleton<MarketplaceStore>())
            .ConfigureActors(ctx =>
            {
                var store = ctx.ServiceProvider.GetRequiredService<MarketplaceStore>();
                var manager = ctx.ServiceProvider.GetRequiredService<IPluginManager>();
                ctx.RegisterActor("marketplace",
                        Props.Create(() => new MarketplaceActor(manager, store)))
                    .WithTicks(minInterval: TimeSpan.FromSeconds(30));
            })
            .ConfigureRoutes(ctx =>
                ctx.RegisterRoute<MarketplacePage, MarketplaceViewModel>("/marketplace"));
    }
}
