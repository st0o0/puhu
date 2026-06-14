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
using Termina.Layout;
using Termina.Pages;

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
        var firstRoute = StartRouteDecider.Decide(SetupWizardState.IsComplete(settings), firstTab);

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
                    return new AppShellNode(themeService, refreshController, layout, hintProvider.GetKeyHints());
                }

                return layout;
            });
        });

        services.AddHostedService(sp => new ShellRedrawService(
            sp.GetRequiredService<ITickSource>(),
            sp.GetRequiredService<IThemeService>(),
            sp.GetRequiredService<IRefreshController>(),
            () => sp.GetRequiredService<TerminaApplication>().RequestRedraw()));

        services.AddSingleton(new StartPageRoute(firstRoute));
    }
}

public sealed record StartPageRoute(string Route);
