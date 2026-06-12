using System.Net;
using System.Text.Json;
using Puhu.Plugin.Marketplace.Models;
using Puhu.Plugin.Marketplace.Services;

namespace Puhu.Marketplace.Tests;

public sealed class PluginMetadataFetcherTests : IDisposable
{
    private readonly string _cacheDir;
    public PluginMetadataFetcherTests()
    {
        _cacheDir = Path.Combine(Path.GetTempPath(), $"servus-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_cacheDir);
    }
    public void Dispose() => Directory.Delete(_cacheDir, true);

    [Fact]
    public async Task FetchRegistryIndex_ValidResponse()
    {
        var index = new RegistryIndex
        {
            Version = 1,
            Plugins = [new RegistryEntry { Id = "servus.plugin.dtop", Repository = "https://github.com/st0o0/dtop" }]
        };
        var handler = new FakeHandler(JsonSerializer.Serialize(index));
        var client = new HttpClient(handler);
        var cache = new PluginCache(_cacheDir);
        var fetcher = new PluginMetadataFetcher(client, cache);
        var result = await fetcher.FetchRegistryIndexAsync("https://example.com/index.json");
        Assert.Single(result.Plugins);
        Assert.Equal("servus.plugin.dtop", result.Plugins[0].Id);
    }

    [Fact]
    public async Task FetchManifest_NotFound_ReturnsNull()
    {
        var handler = new FakeHandler(statusCode: HttpStatusCode.NotFound);
        var client = new HttpClient(handler);
        var cache = new PluginCache(_cacheDir);
        var fetcher = new PluginMetadataFetcher(client, cache);
        var result = await fetcher.FetchManifestAsync("https://github.com/st0o0/nonexistent");
        Assert.Null(result);
    }

    private sealed class FakeHandler(string? content = null, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var response = new HttpResponseMessage(statusCode);
            if (content is not null) response.Content = new StringContent(content);
            return Task.FromResult(response);
        }
    }
}
