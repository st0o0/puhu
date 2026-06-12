using System.Text.Json;
using Puhu.Plugin.Marketplace.Models;

namespace Puhu.Plugin.Marketplace.Services;

public sealed class PluginDownloader(HttpClient httpClient, string pluginsDir)
{
    public async Task<string> DownloadAsync(string pluginId, string repositoryUrl, PluginDelivery delivery)
    {
        var targetDir = Path.Combine(pluginsDir, pluginId);
        Directory.CreateDirectory(targetDir);
        return delivery.Type switch
        {
            DeliveryType.GitHubRelease => await DownloadFromGitHubAsync(repositoryUrl, delivery.Asset!, targetDir),
            DeliveryType.NuGet => throw new NotSupportedException("NuGet delivery not yet implemented"),
            _ => throw new NotSupportedException($"Delivery type {delivery.Type} is not supported")
        };
    }

    public void Remove(string pluginId)
    {
        var dir = Path.Combine(pluginsDir, pluginId);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    private async Task<string> DownloadFromGitHubAsync(string repositoryUrl, string assetName, string targetDir)
    {
        var uri = new Uri(repositoryUrl.TrimEnd('/'));
        var apiUrl = $"https://api.github.com/repos{uri.AbsolutePath}/releases/latest";
        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        request.Headers.Add("User-Agent", "servus-plugin-manager");
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        string? downloadUrl = null;
        foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
        {
            if (asset.GetProperty("name").GetString() == assetName)
            {
                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                break;
            }
        }
        if (downloadUrl is null)
        {
            throw new InvalidOperationException($"Asset '{assetName}' not found in latest release");
        }

        var bytes = await httpClient.GetByteArrayAsync(downloadUrl);
        var targetPath = Path.Combine(targetDir, assetName);
        await File.WriteAllBytesAsync(targetPath, bytes);
        return targetPath;
    }
}
