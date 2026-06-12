using Akka.Actor;
using Akka.Hosting;
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
            .WithActors((system, registry, resolver) =>
            {
                var actor = system.ActorOf(resolver.Props<MarketplaceActor>(), "marketplace");
                registry.Register<MarketplaceActor>(actor);

                var tickRouter = registry.Get<TickRouterKey>();
                tickRouter.Tell(new RegisterMonitor("marketplace", actor, false, TimeSpan.FromSeconds(30)));
            })
            .WithRoutes(termina => termina.RegisterRoute<MarketplacePage, MarketplaceViewModel>("/marketplace"));
    }
}