using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Servus.Application.Startup;
using Termina;
using Termina.Hosting;
using Termina.Pages;

namespace Puhu.Setup;

public sealed class TerminaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();
        var pluginRegistry = ctx.PluginRegistry;
        var firstRoute = pluginRegistry?.PluginTabs.FirstOrDefault()?.Route ?? "/marketplace";

        services.AddTermina(termina =>
        {
            termina.ConfigureRuntime(x =>
            {
                x.PreferRawInput = true;
                x.ScrollInputMode = ScrollInputMode.AlternateScroll;
                x.CtrlCHandlingMode = CtrlCHandlingMode.DoublePressWhenRawInput;
            });

            if (pluginRegistry is not null)
            {
                var routeCtx = new PluginRouteContext(termina);
                foreach (var plugin in pluginRegistry.LoadedPlugins)
                {
                    plugin.RouteSetup?.Invoke(routeCtx);
                }
            }
        });

        services.AddSingleton(new StartPageRoute(firstRoute));
        services.AddHostedService<StartPageNavigator>();
    }
}

public sealed record StartPageRoute(string Route);

public sealed class StartPageNavigator(TerminaApplication app, StartPageRoute startPage) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        app.NavigateTo(startPage.Route);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
