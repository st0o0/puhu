using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        var themeService = new ThemeService();
        themeService.LoadFromDirectory(Path.Combine(AppContext.BaseDirectory, "themes"));
        themeService.ApplyBuiltIn("dark");
        services.AddSingleton(themeService);
        services.AddSingleton<IThemeService>(themeService);

        var ctx = new SetupContext { TickSource = refreshService };
        services.AddSingleton(ctx);
    }
}
