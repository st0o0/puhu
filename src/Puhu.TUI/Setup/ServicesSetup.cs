using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Puhu.Plugin;
using Puhu.TUI.Services;

namespace Puhu.TUI.Setup;

public sealed class ServicesSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var refreshService = new RefreshService(TimeSpan.FromMilliseconds(1000));
        services.AddSingleton(refreshService);
        services.AddSingleton<ITickSource>(refreshService);

        var ctx = new SetupContext { TickSource = refreshService };
        services.AddSingleton(ctx);
    }
}
