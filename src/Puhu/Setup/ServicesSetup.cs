using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Services;
using Puhu.Themes;
using Servus.Application.Startup;

namespace Puhu.Setup;

public sealed class ServicesSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var refreshService = new RefreshService(TimeSpan.FromMilliseconds(1000));
        services.AddSingleton(refreshService);
        services.AddSingleton<ITickSource>(refreshService);

        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".servus", "settings.json");
        var settingsStore = new SettingsStore(settingsPath);
        services.AddSingleton(settingsStore);

        var themeService = new ThemeService(settingsStore);
        themeService.LoadFromDirectory(Path.Combine(AppContext.BaseDirectory, "Themes"));
        if (!themeService.RestoreSaved() && !themeService.ApplyByName("btop-default"))
        {
            themeService.ApplyBuiltIn("dark");
        }

        services.AddSingleton(themeService);
        services.AddSingleton<IThemeService>(themeService);

        services.AddSingleton<ITabNavigator>(new TabNavigator());

        var ctx = new SetupContext();
        services.AddSingleton(ctx);
    }
}
