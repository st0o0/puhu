using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Servus.Application.Startup;
using Servus.Plugin.Marketplace.Models;
using Servus.Plugin.Marketplace.Services;

namespace Servus.TUI.Setup;

public sealed class MarketplaceSetup : IServiceSetupContainer
{
    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".servus");
        var pluginsDir = Path.Combine(basePath, "plugins");

        var configStore = new PluginConfigStore(basePath);
        var cache = new PluginCache(Path.Combine(basePath, "cache"));
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "servus-plugin-manager");
        var fetcher = new PluginMetadataFetcher(httpClient, cache);
        var downloader = new PluginDownloader(httpClient, pluginsDir);
        var manager = new PluginManager(configStore, fetcher, downloader, pluginsDir);

        services.AddSingleton(configStore);
        services.AddSingleton<IPluginManager>(manager);
    }
}
