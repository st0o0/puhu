using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Pages;
using Servus.Application.Startup;
using Termina.Hosting;

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
    }
}

public sealed record StartPageRoute(string Route);
