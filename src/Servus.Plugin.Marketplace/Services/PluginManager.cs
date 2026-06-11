using Servus.Plugin.Marketplace.Models;

namespace Servus.Plugin.Marketplace.Services;

public sealed class PluginManager(
    PluginConfigStore configStore,
    PluginMetadataFetcher fetcher,
    PluginDownloader downloader,
    string pluginsDir) : IPluginManager
{
    private InstalledPluginsFile? _installedCache;

    public async Task<IReadOnlyList<PluginInfo>> FetchAvailableAsync(bool ignoreCache = false)
    {
        var sources = await configStore.LoadSourcesAsync();
        var installed = await LoadInstalledAsync();
        var installedLookup = installed.Plugins.ToDictionary(p => p.Id);
        var allRepoUrls = new List<string>();
        foreach (var registryUrl in sources.Registries)
        {
            var index = await fetcher.FetchRegistryIndexAsync(registryUrl, ignoreCache);
            allRepoUrls.AddRange(index.Plugins.Select(e => e.Repository));
        }
        allRepoUrls.AddRange(sources.Repositories);
        var results = new List<PluginInfo>();
        foreach (var repoUrl in allRepoUrls)
        {
            var manifest = await fetcher.FetchManifestAsync(repoUrl, ignoreCache);
            if (manifest is null)
            {
                continue;
            }

            var installedPlugin = installedLookup.GetValueOrDefault(manifest.Id);
            results.Add(PluginInfo.FromManifest(manifest, installedPlugin?.Version, installedPlugin?.UpdatePolicy));
        }
        return results;
    }

    public async Task AddSourceAsync(string repoUrl)
    {
        var sources = await configStore.LoadSourcesAsync();
        if (!sources.Repositories.Contains(repoUrl))
        {
            sources.Repositories.Add(repoUrl);
            await configStore.SaveSourcesAsync(sources);
        }
    }

    public async Task RemoveSourceAsync(string repoUrl)
    {
        var sources = await configStore.LoadSourcesAsync();
        sources.Repositories.Remove(repoUrl);
        await configStore.SaveSourcesAsync(sources);
    }

    public async Task InstallAsync(string pluginId)
    {
        var available = await FetchAvailableAsync();
        var plugin = available.FirstOrDefault(p => p.Id == pluginId)
            ?? throw new InvalidOperationException($"Plugin '{pluginId}' not found");
        var path = await downloader.DownloadAsync(pluginId, plugin.Repository, plugin.Delivery);
        var installed = await LoadInstalledAsync();
        installed.Plugins.Add(new InstalledPlugin
        {
            Id = pluginId, Version = plugin.AvailableVersion, Source = plugin.Repository,
            UpdatePolicy = UpdatePolicy.Auto,
            Path = Path.GetRelativePath(Path.GetDirectoryName(pluginsDir)!, path)
        });
        await SaveInstalledAsync(installed);
    }

    public async Task UpdateAsync(string pluginId)
    {
        var installed = await LoadInstalledAsync();
        var plugin = installed.Plugins.FirstOrDefault(p => p.Id == pluginId)
            ?? throw new InvalidOperationException($"Plugin '{pluginId}' is not installed");
        var manifest = await fetcher.FetchManifestAsync(plugin.Source, ignoreCache: true)
            ?? throw new InvalidOperationException($"Could not fetch manifest for '{pluginId}'");
        downloader.Remove(pluginId);
        var path = await downloader.DownloadAsync(pluginId, plugin.Source, manifest.Delivery);
        var idx = installed.Plugins.FindIndex(p => p.Id == pluginId);
        installed.Plugins[idx] = plugin with
        {
            Version = manifest.Version,
            Path = Path.GetRelativePath(Path.GetDirectoryName(pluginsDir)!, path)
        };
        await SaveInstalledAsync(installed);
    }

    public async Task UninstallAsync(string pluginId)
    {
        downloader.Remove(pluginId);
        var installed = await LoadInstalledAsync();
        installed.Plugins.RemoveAll(p => p.Id == pluginId);
        await SaveInstalledAsync(installed);
    }

    public async Task SyncAllAsync()
    {
        var installed = await LoadInstalledAsync();
        foreach (var plugin in installed.Plugins.Where(p => p.UpdatePolicy == UpdatePolicy.Auto).ToList())
        {
            var manifest = await fetcher.FetchManifestAsync(plugin.Source, ignoreCache: true);
            if (manifest is not null && Version.Parse(manifest.Version) > Version.Parse(plugin.Version))
            {
                await UpdateAsync(plugin.Id);
            }
        }
    }

    public IReadOnlyList<InstalledPlugin> GetInstalled() =>
        configStore.LoadInstalledAsync().GetAwaiter().GetResult().Plugins;

    public async Task SetUpdatePolicyAsync(string pluginId, UpdatePolicy policy)
    {
        var installed = await LoadInstalledAsync();
        var plugin = installed.Plugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin is null)
        {
            return;
        }

        plugin.UpdatePolicy = policy;
        await SaveInstalledAsync(installed);
    }

    private async Task<InstalledPluginsFile> LoadInstalledAsync()
    {
        _installedCache ??= await configStore.LoadInstalledAsync();
        return _installedCache;
    }

    private async Task SaveInstalledAsync(InstalledPluginsFile installed)
    {
        _installedCache = installed;
        await configStore.SaveInstalledAsync(installed);
    }
}
