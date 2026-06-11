using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Servus.TUI.Actors;

namespace Servus.TUI.Setup;

public sealed class AkkaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();
        var pluginRegistry = ctx.PluginRegistry
            ?? throw new InvalidOperationException(
                "PluginRegistry must be set before AkkaSetup. Ensure PluginSetup runs first.");

        services.AddAkka("servus", builder =>
        {
            builder.WithActors((system, registry, resolver) =>
            {
                var tickRouter = registry.Get<TickRouter>();
                var sp = resolver.GetService<IServiceProvider>();

                foreach (var plugin in pluginRegistry.LoadedPlugins)
                {
                    if (plugin.ActorSetup is null) continue;

                    var actorCtx = new PluginActorContextImpl(sp, plugin.ActorRegistrations);
                    plugin.ActorSetup.Invoke(actorCtx);

                    foreach (var reg in plugin.ActorRegistrations)
                    {
                        var actorRef = system.ActorOf(reg.Props, reg.Name);

                        if (reg is { AlwaysOn: true } or { MinInterval: not null })
                        {
                            tickRouter.Tell(new RegisterMonitor(
                                reg.Name, actorRef, reg.AlwaysOn, reg.MinInterval));
                        }
                    }
                }
            });
        });
    }
}
