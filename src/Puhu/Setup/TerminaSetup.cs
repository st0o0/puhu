using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Puhu.Pages;
using R3;
using Servus.Application.Startup;
using Termina;
using Termina.Hosting;
using Termina.Input;
using Termina.Pages;

namespace Puhu.Setup;

public sealed class TerminaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();
        var pluginRegistry = ctx.PluginRegistry;
        var firstRoute = pluginRegistry?.PluginTabs.FirstOrDefault()?.Route ?? "/marketplace";

        services.AddTermina("/splash", termina =>
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

            termina.RegisterRoute<SplashPage, SplashViewModel>("/splash");
        });

        services.AddSingleton(new StartPageRoute(firstRoute));
        services.AddHostedService<GlobalKeyHandler>();
    }
}

public sealed record StartPageRoute(string Route);

public sealed class GlobalKeyHandler(TerminaApplication app) : IHostedService, IDisposable
{
    private IDisposable? _subscription;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscription = app.Input.OfType<IInputEvent, KeyPressed>()
            .Where(k => k.KeyInfo.Key == ConsoleKey.Escape)
            .Subscribe(_ => app.Shutdown());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose() => _subscription?.Dispose();
}
