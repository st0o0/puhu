using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Nodes;
using Puhu.Pages;
using Puhu.Plugin;
using Puhu.Services;
using Puhu.Settings;
using Servus.Application.Startup;
using Termina;
using Termina.Hosting;

namespace Puhu.Setup;

public sealed class TerminaSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var sp = services.BuildServiceProvider();
        var ctx = sp.GetRequiredService<SetupContext>();
        var pluginRegistry = ctx.PluginRegistry;
        var themeService = sp.GetRequiredService<IThemeService>();
        var refreshController = sp.GetRequiredService<IRefreshController>();
        var settings = sp.GetRequiredService<ISettingsStore>();
        var tabOrder = sp.GetRequiredService<ITabOrderService>();

        var firstTab = tabOrder.Tabs.FirstOrDefault()?.Route ?? "/marketplace";
        var firstRoute = StartRouteDecider.Decide(settings.IsWizardComplete(), firstTab);

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
                if (page is not IKeyHintProvider hintProvider)
                    return layout;

                var globalHints = Array.Empty<string>();
                if (hintProvider.ShowGlobalHints)
                {
                    globalHints = refreshController is not null
                        ? ["Esc:Quit", "Tab:Switch", "+/-:Speed", "P:Pause"]
                        : ["Esc:Quit", "Tab:Switch"];
                }

                return new AppShellNode(
                    themeService, refreshController, layout,
                    hintProvider.GetKeyHints(), globalHints);
            });
        });

        services.AddHostedService(provider => new ShellRedrawService(
            provider.GetRequiredService<ITickSource>(),
            provider.GetRequiredService<IThemeService>(),
            provider.GetRequiredService<IRefreshController>(),
            () => provider.GetRequiredService<TerminaApplication>().RequestRedraw()));

        services.AddSingleton(new StartPageRoute(firstRoute));
    }
}

public sealed record StartPageRoute(string Route);