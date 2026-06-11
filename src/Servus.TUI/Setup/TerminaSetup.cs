using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Termina.Hosting;
using Termina.Pages;

namespace Servus.TUI.Setup;

public sealed class TerminaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var pluginRegistry = GetRegisteredSingleton<PluginRegistry>(services);
        var firstRoute = pluginRegistry?.PluginTabs.FirstOrDefault()?.Route ?? "/marketplace";

        services.AddTermina(firstRoute, termina =>
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
    }

    private static T? GetRegisteredSingleton<T>(IServiceCollection services) where T : class
    {
        var descriptor = services.LastOrDefault(d =>
            d.ServiceType == typeof(T) && d.Lifetime == ServiceLifetime.Singleton);

        return descriptor?.ImplementationInstance as T;
    }
}
