using Akka.Actor;
using Akka.Hosting;
using Akka.Logger.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Actors;
using Puhu.Services;
using R3;
using Servus.Application.Startup;

namespace Puhu.Setup;

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

                foreach (var plugin in pluginRegistry.LoadedPlugins)
                {
                    plugin.ActorSetup?.Invoke(system, registry, resolver);
                }
            });
        });
    }
}
