using System.Text.Json;
using Servus.Plugin.Marketplace.Models;

namespace Servus.Plugin.Marketplace.Services;

public sealed class PluginConfigStore(string basePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private string SourcesPath => Path.Combine(basePath, "sources.json");
    private string InstalledPath => Path.Combine(basePath, "installed.json");

    public async Task<PluginSources> LoadSourcesAsync()
    {
        if (!File.Exists(SourcesPath)) return new PluginSources();
        await using var stream = File.OpenRead(SourcesPath);
        return await JsonSerializer.DeserializeAsync<PluginSources>(stream) ?? new PluginSources();
    }

    public async Task SaveSourcesAsync(PluginSources sources)
    {
        Directory.CreateDirectory(basePath);
        await using var stream = File.Create(SourcesPath);
        await JsonSerializer.SerializeAsync(stream, sources, JsonOptions);
    }

    public async Task<InstalledPluginsFile> LoadInstalledAsync()
    {
        if (!File.Exists(InstalledPath)) return new InstalledPluginsFile();
        await using var stream = File.OpenRead(InstalledPath);
        return await JsonSerializer.DeserializeAsync<InstalledPluginsFile>(stream) ?? new InstalledPluginsFile();
    }

    public async Task SaveInstalledAsync(InstalledPluginsFile installed)
    {
        Directory.CreateDirectory(basePath);
        await using var stream = File.Create(InstalledPath);
        await JsonSerializer.SerializeAsync(stream, installed, JsonOptions);
    }

    public async Task EnsureDefaultSourcesAsync()
    {
        if (File.Exists(SourcesPath)) return;
        var defaults = new PluginSources
        {
            Registries = ["https://raw.githubusercontent.com/st0o0/servus.registry/main/index.json"]
        };
        await SaveSourcesAsync(defaults);
    }
}
