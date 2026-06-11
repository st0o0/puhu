namespace Servus.Plugin.Marketplace.Models;

public interface IPluginManager
{
    Task<IReadOnlyList<PluginInfo>> FetchAvailableAsync(bool ignoreCache = false);
    Task AddSourceAsync(string repoUrl);
    Task RemoveSourceAsync(string repoUrl);
    Task InstallAsync(string pluginId);
    Task UpdateAsync(string pluginId);
    Task UninstallAsync(string pluginId);
    Task SyncAllAsync();
    IReadOnlyList<InstalledPlugin> GetInstalled();
    Task SetUpdatePolicyAsync(string pluginId, UpdatePolicy policy);
}
