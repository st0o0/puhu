using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin.Marketplace.Models;
using Puhu.Plugin.Marketplace.Services;
using Servus.Application.Startup;

namespace Puhu.Setup;

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
