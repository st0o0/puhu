using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Puhu.Pages;
using R3;
using Servus.Application.Startup;
using Termina;
using Termina.Hosting;
using Termina.Input;

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
                foreach (var plugin in pluginRegistry.LoadedPlugins)
                {
                    plugin.RouteSetup?.Invoke(termina);
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
    private int _currentTab;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscription = app.Input.OfType<IInputEvent, KeyPressed>()
            .Subscribe(HandleKey);
        return Task.CompletedTask;
    }

    private void HandleKey(KeyPressed key)
    {
        switch (key.KeyInfo.Key)
        {
            case ConsoleKey.Escape:
                app.Shutdown();
                break;

            case ConsoleKey.Tab when Plugin.Nodes.TabBarNode.TabCount > 0:
                try
                {
                    var delta = key.KeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) ? -1 : 1;
                    var count = Plugin.Nodes.TabBarNode.TabCount;
                    _currentTab = (_currentTab + delta + count) % count;
                    app.NavigateTo(Plugin.Nodes.TabBarNode.GetRoute(_currentTab));
                }
                catch (InvalidOperationException)
                {
                }
                break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose() => _subscription?.Dispose();
}
