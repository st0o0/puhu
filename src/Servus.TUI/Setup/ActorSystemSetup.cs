using Akka.Actor;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Servus.Application.Startup;
using Servus.Plugin;
using Servus.TUI.Actors;
using Servus.TUI.Services;

namespace Servus.TUI.Setup;

public sealed class ActorSystemSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAkka("servus", builder =>
        {
            builder.ConfigureLoggers(logging =>
            {
                logging.LogLevel = Akka.Event.LogLevel.WarningLevel;
                logging.AddSerilogLogging();
                logging.DeadLetterOptions = new DeadLetterOptions
                {
                    ShouldLog = TriStateValue.None,
                    LogDuringShutdown = false
                };
            });

            builder.AddHocon("akka.stdout-loglevel = Off", HoconAddMode.Prepend);

            builder.WithActors((system, registry, resolver) =>
            {
                var tickRouter = system.ActorOf(Props.Create<TickRouter>(), "tick-router");
                registry.Register<TickRouter>(tickRouter);

                var refreshService = resolver.GetService<RefreshService>();
                refreshService.Ticks.Subscribe(t => tickRouter.Tell(t));

                var pluginRegistry = resolver.GetService<PluginRegistry>();
                var sp = resolver.GetService<IServiceProvider>();
                foreach (var plugin in pluginRegistry.LoadedPlugins)
                {
                    var actorCtx = new PluginActorContextImpl(sp, plugin.ActorRegistrations);
                    plugin.ActorSetup?.Invoke(actorCtx);
                }
            });
        });
    }
}
