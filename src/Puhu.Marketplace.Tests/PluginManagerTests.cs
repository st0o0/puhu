// tests/Puhu.Marketplace.Tests/PluginManagerTests.cs
using System.Net;
using System.Text.Json;
using Puhu.Marketplace.Models;
using Puhu.Marketplace.Services;

namespace Puhu.Marketplace.Tests;

public sealed class PluginManagerTests : IDisposable
{
    private readonly string _baseDir;
    private readonly string _pluginsDir;
    private readonly PluginConfigStore _configStore;
    private readonly PluginCache _cache;

    public PluginManagerTests()
    {
        _baseDir = Path.Combine(Path.GetTempPath(), $"servus-mgr-{Guid.NewGuid():N}");
        _pluginsDir = Path.Combine(_baseDir, "plugins");
        Directory.CreateDirectory(_baseDir);
        Directory.CreateDirectory(_pluginsDir);
        _configStore = new PluginConfigStore(_baseDir);
        _cache = new PluginCache(Path.Combine(_baseDir, "cache"));
    }

    public void Dispose() => Directory.Delete(_baseDir, true);

    [Fact]
    public async Task FetchAvailable_MergesRegistryAndManualSources()
    {
        var sources = new PluginSources
        {
            Registries = ["https://example.com/index.json"],
            Repositories = ["https://github.com/test/manual-plugin"]
        };
        await _configStore.SaveSourcesAsync(sources);

        var handler = new TestHandler();
        handler.AddRegistryIndex("https://example.com/index.json", new RegistryIndex
        {
            Version = 1,
            Plugins = [new RegistryEntry { Id = "servus.plugin.dtop", Repository = "https://github.com/st0o0/dtop" }]
        });
        handler.AddManifest("st0o0/dtop", CreateManifest("servus.plugin.dtop", "1.0.0"));
        handler.AddManifest("test/manual-plugin", CreateManifest("servus.plugin.manual", "0.5.0"));

        var manager = CreateManager(handler);
        var available = await manager.FetchAvailableAsync(ignoreCache: true);

        Assert.Equal(2, available.Count);
        Assert.Contains(available, p => p.Id == "servus.plugin.dtop");
        Assert.Contains(available, p => p.Id == "servus.plugin.manual");
    }

    [Fact]
    public async Task AddSource_PersistsToFile()
    {
        var manager = CreateManager(new TestHandler());
        await manager.AddSourceAsync("https://github.com/test/new-plugin");
        var sources = await _configStore.LoadSourcesAsync();
        Assert.Contains("https://github.com/test/new-plugin", sources.Repositories);
    }

    [Fact]
    public async Task RemoveSource_RemovesFromFile()
    {
        await _configStore.SaveSourcesAsync(new PluginSources
        {
            Repositories = ["https://github.com/test/a", "https://github.com/test/b"]
        });
        var manager = CreateManager(new TestHandler());
        await manager.RemoveSourceAsync("https://github.com/test/a");
        var sources = await _configStore.LoadSourcesAsync();
        Assert.Single(sources.Repositories);
        Assert.Equal("https://github.com/test/b", sources.Repositories[0]);
    }

    [Fact]
    public async Task SetUpdatePolicy_PersistsToFile()
    {
        await _configStore.SaveInstalledAsync(new InstalledPluginsFile
        {
            Plugins = [new InstalledPlugin
            {
                Id = "servus.plugin.dtop", Version = "1.0.0",
                Source = "https://github.com/st0o0/dtop",
                UpdatePolicy = UpdatePolicy.Auto,
                Path = "plugins/servus.plugin.dtop/Puhu.Plugin.Dtop.dll"
            }]
        });
        var manager = CreateManager(new TestHandler());
        await manager.SetUpdatePolicyAsync("servus.plugin.dtop", UpdatePolicy.Pinned);
        var installed = await _configStore.LoadInstalledAsync();
        Assert.Equal(UpdatePolicy.Pinned, installed.Plugins[0].UpdatePolicy);
    }

    private PluginManager CreateManager(TestHandler handler)
    {
        var client = new HttpClient(handler);
        var fetcher = new PluginMetadataFetcher(client, _cache);
        var downloader = new PluginDownloader(client, _pluginsDir);
        return new PluginManager(_configStore, fetcher, downloader, _pluginsDir);
    }

    private static PluginManifest CreateManifest(string id, string version) => new()
    {
        Id = id, Name = id, Description = "Test", Author = "test", Version = version,
        MinPuhuVersion = "1.0.0", License = "MIT",
        Repository = $"https://github.com/test/{id}",
        Delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = $"{id}.dll" }
    };

    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses = new();
        public void AddRegistryIndex(string url, RegistryIndex index) => _responses[url] = JsonSerializer.Serialize(index);
        public void AddManifest(string repoPath, PluginManifest manifest) => _responses[$"https://raw.githubusercontent.com/{repoPath}/main/puhu-manifest.json"] = JsonSerializer.Serialize(manifest);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var url = request.RequestUri!.ToString();
            return _responses.TryGetValue(url, out var content)
                ? Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) })
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
