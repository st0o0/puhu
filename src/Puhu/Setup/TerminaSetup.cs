using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Nodes;
using Puhu.Pages;
using Puhu.Plugin;
using Servus.Application.Startup;
using Termina.Hosting;
using Termina.Layout;
using Termina.Pages;

namespace Puhu.Setup;

public sealed class TerminaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var ctx = services.BuildServiceProvider().GetRequiredService<SetupContext>();
        var pluginRegistry = ctx.PluginRegistry;
        var themeService = services.BuildServiceProvider().GetRequiredService<IThemeService>();
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

            termina.UseLayoutDecorator((page, layout) =>
            {
                if (page is IKeyHintProvider hintProvider)
                {
                    var hints = hintProvider.GetKeyHints();
                    return new AppShellNode(themeService.Current, TabBarNode.CurrentTabIndex, layout, hints);
                }

                return layout;
            });
        });

        services.AddSingleton(new StartPageRoute(firstRoute));
    }
}

public sealed record StartPageRoute(string Route);
