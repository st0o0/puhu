using System.Net;
using Puhu.Marketplace.Models;
using Puhu.Marketplace.Services;

namespace Puhu.Marketplace.Tests;

public sealed class PluginDownloaderTests : IDisposable
{
    private readonly string _pluginsDir;
    public PluginDownloaderTests()
    {
        _pluginsDir = Path.Combine(Path.GetTempPath(), $"servus-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_pluginsDir);
    }
    public void Dispose() => Directory.Delete(_pluginsDir, true);

    [Fact]
    public async Task Download_GithubRelease_SavesDll()
    {
        var fakeBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 };
        var handler = new FakeDownloadHandler(fakeBytes);
        var client = new HttpClient(handler);
        var downloader = new PluginDownloader(client, _pluginsDir);
        var delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = "Puhu.Plugin.Dtop.dll" };
        var dir = await downloader.DownloadAsync("servus.plugin.dtop", "https://github.com/st0o0/dtop", delivery);
        var filePath = Path.Combine(dir, "Puhu.Plugin.Dtop.dll");
        Assert.True(File.Exists(filePath));
        Assert.Equal(fakeBytes, await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Download_CreatesPluginSubdirectory()
    {
        var handler = new FakeDownloadHandler([0x4D, 0x5A]);
        var client = new HttpClient(handler);
        var downloader = new PluginDownloader(client, _pluginsDir);
        var delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = "Puhu.Plugin.Dtop.dll" };
        var path = await downloader.DownloadAsync("servus.plugin.dtop", "https://github.com/st0o0/dtop", delivery);
        var expectedDir = Path.Combine(_pluginsDir, "servus.plugin.dtop");
        Assert.True(Directory.Exists(expectedDir));
    }

    [Fact]
    public async Task Remove_DeletesPluginDirectory()
    {
        var handler = new FakeDownloadHandler([0x4D, 0x5A]);
        var client = new HttpClient(handler);
        var downloader = new PluginDownloader(client, _pluginsDir);
        var delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = "Puhu.Plugin.Dtop.dll" };
        await downloader.DownloadAsync("servus.plugin.dtop", "https://github.com/st0o0/dtop", delivery);
        downloader.Remove("servus.plugin.dtop");
        Assert.False(Directory.Exists(Path.Combine(_pluginsDir, "servus.plugin.dtop")));
    }

    private sealed class FakeDownloadHandler(byte[] content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.RequestUri!.AbsolutePath.Contains("/releases/latest"))
            {
                const string json = """{"assets": [{"name": "Puhu.Plugin.Dtop.dll", "browser_download_url": "https://download.example.com/Puhu.Plugin.Dtop.dll"}]}""";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });
        }
    }
}
