using Puhu.Plugin.Marketplace.Models;
using Puhu.Plugin.Marketplace.Services;

namespace Puhu.Marketplace.Tests;

public sealed class PluginConfigStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly PluginConfigStore _store;

    public PluginConfigStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"servus-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _store = new PluginConfigStore(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, true);

    [Fact]
    public async Task LoadSources_NoFile_ReturnsDefault()
    {
        var sources = await _store.LoadSourcesAsync();
        Assert.Empty(sources.Registries);
        Assert.Empty(sources.Repositories);
    }

    [Fact]
    public async Task SaveAndLoadSources_RoundTrips()
    {
        var sources = new PluginSources
        {
            Registries = ["https://example.com/index.json"],
            Repositories = ["https://github.com/test/plugin"]
        };
        await _store.SaveSourcesAsync(sources);
        var loaded = await _store.LoadSourcesAsync();
        Assert.Single(loaded.Registries);
        Assert.Equal("https://example.com/index.json", loaded.Registries[0]);
    }

    [Fact]
    public async Task LoadInstalled_NoFile_ReturnsEmpty()
    {
        var installed = await _store.LoadInstalledAsync();
        Assert.Empty(installed.Plugins);
    }

    [Fact]
    public async Task SaveAndLoadInstalled_RoundTrips()
    {
        var installed = new InstalledPluginsFile
        {
            Plugins = [new InstalledPlugin
            {
                Id = "servus.plugin.dtop", Version = "1.0.0",
                Source = "https://github.com/st0o0/dtop",
                UpdatePolicy = UpdatePolicy.Auto,
                Path = "plugins/servus.plugin.dtop/Puhu.Plugin.Dtop.dll"
            }]
        };
        await _store.SaveInstalledAsync(installed);
        var loaded = await _store.LoadInstalledAsync();
        Assert.Single(loaded.Plugins);
        Assert.Equal("servus.plugin.dtop", loaded.Plugins[0].Id);
        Assert.Equal(UpdatePolicy.Auto, loaded.Plugins[0].UpdatePolicy);
    }

    [Fact]
    public async Task EnsureDefaultSources_CreatesFileOnFirstCall()
    {
        await _store.EnsureDefaultSourcesAsync();
        var sources = await _store.LoadSourcesAsync();
        Assert.Single(sources.Registries);
        Assert.Contains("servus.registry", sources.Registries[0]);
    }

    [Fact]
    public async Task EnsureDefaultSources_DoesNotOverwriteExisting()
    {
        var custom = new PluginSources { Registries = ["https://custom.example.com/index.json"] };
        await _store.SaveSourcesAsync(custom);
        await _store.EnsureDefaultSourcesAsync();
        var sources = await _store.LoadSourcesAsync();
        Assert.Single(sources.Registries);
        Assert.Equal("https://custom.example.com/index.json", sources.Registries[0]);
    }
}
