using Puhu.Marketplace;

namespace Puhu.Marketplace.Models;

public interface IPluginManager
{
    Task<IReadOnlyList<PluginInfo>> FetchAvailableAsync(bool ignoreCache = false);
    Task<PluginSources> GetSourcesAsync();
    Task EnsureDefaultSourcesAsync();
    Task AddSourceAsync(string url, SourceType type);
    Task RemoveSourceAsync(string repoUrl);
    Task InstallAsync(string pluginId);
    Task UpdateAsync(string pluginId);
    Task UninstallAsync(string pluginId);
    Task SyncAllAsync();
    Task SetUpdatePolicyAsync(string pluginId, UpdatePolicy policy);
}
