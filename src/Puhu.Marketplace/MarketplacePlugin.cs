using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Marketplace.Actors;
using Puhu.Marketplace.Models;
using Puhu.Marketplace.Pages;
using Puhu.Plugin;

namespace Puhu.Marketplace;

public sealed class MarketplacePlugin : IPuhuPlugin
{
    public string Name => "Marketplace";

    public void Configure(IPuhuPluginBuilder builder)
    {
        builder
            .WithTab("Marketplace", "/marketplace")
            .WithServices(services => services.AddSingleton<MarketplaceStore>())
            .ConfigureActors(ctx =>
            {
                var store = ctx.ServiceProvider.GetRequiredService<MarketplaceStore>();
                var manager = ctx.ServiceProvider.GetRequiredService<IPluginManager>();
                ctx.RegisterActor<MarketplaceActor>("marketplace",
                        Props.Create(() => new MarketplaceActor(manager, store)))
                    .WithTicks(minInterval: TimeSpan.FromSeconds(30));
            })
            .ConfigureRoutes(ctx =>
                ctx.RegisterRoute<MarketplacePage, MarketplaceViewModel>("/marketplace"));
    }
}
