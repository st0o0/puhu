using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Servus.Plugin;
using Servus.TUI.Services;

namespace Servus.TUI.Setup;

public sealed class ServicesSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var refreshService = new RefreshService(TimeSpan.FromMilliseconds(1000));
        services.AddSingleton(refreshService);
        services.AddSingleton<ITickSource>(refreshService);
    }
}
