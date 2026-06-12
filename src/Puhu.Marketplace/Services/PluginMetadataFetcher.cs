using System.Text.Json;
using Puhu.Marketplace.Models;

namespace Puhu.Marketplace.Services;

public sealed class PluginMetadataFetcher(HttpClient httpClient, PluginCache cache)
{
    public async Task<RegistryIndex> FetchRegistryIndexAsync(string registryUrl, bool ignoreCache = false)
    {
        var cacheKey = $"registry_{Uri.EscapeDataString(registryUrl)}";
        if (!ignoreCache)
        {
            var cached = await cache.GetAsync<RegistryIndex>(cacheKey);
            if (cached is not null)
            {
                return cached;
            }
        }
        var response = await httpClient.GetAsync(registryUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var index = JsonSerializer.Deserialize<RegistryIndex>(content) ?? new RegistryIndex { Version = 1, Plugins = [] };
        await cache.SetAsync(cacheKey, index);
        return index;
    }

    public async Task<PluginManifest?> FetchManifestAsync(string repositoryUrl, bool ignoreCache = false)
    {
        var cacheKey = $"manifest_{Uri.EscapeDataString(repositoryUrl)}";
        if (!ignoreCache)
        {
            var cached = await cache.GetAsync<PluginManifest>(cacheKey);
            if (cached is not null)
            {
                return cached;
            }
        }
        var manifestUrl = ToRawManifestUrl(repositoryUrl);
        try
        {
            var response = await httpClient.GetAsync(manifestUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var manifest = JsonSerializer.Deserialize<PluginManifest>(content);
            if (manifest is not null)
            {
                await cache.SetAsync(cacheKey, manifest);
            }

            return manifest;
        }
        catch (HttpRequestException) { return null; }
    }

    private static string ToRawManifestUrl(string repositoryUrl)
    {
        var uri = new Uri(repositoryUrl.TrimEnd('/'));
        if (uri.Host == "github.com")
        {
            return $"https://raw.githubusercontent.com{uri.AbsolutePath}/main/servus-plugin.json";
        }

        return $"{repositoryUrl.TrimEnd('/')}/servus-plugin.json";
    }
}
