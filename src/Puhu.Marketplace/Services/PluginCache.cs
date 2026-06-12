using System.Text.Json;

namespace Puhu.Marketplace.Services;

public sealed class PluginCache(string cacheDir)
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var path = GetPath(key);
        var metaPath = path + ".meta";
        if (!File.Exists(path) || !File.Exists(metaPath))
        {
            return null;
        }

        var meta = await File.ReadAllTextAsync(metaPath);
        if (DateTimeOffset.TryParse(meta, out var ts) && DateTimeOffset.UtcNow - ts > DefaultTtl)
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        Directory.CreateDirectory(cacheDir);
        var path = GetPath(key);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value);
        await File.WriteAllTextAsync(path + ".meta", DateTimeOffset.UtcNow.ToString("O"));
    }

    public void InvalidateAll()
    {
        if (!Directory.Exists(cacheDir))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(cacheDir))
        {
            File.Delete(file);
        }
    }

    private string GetPath(string key)
    {
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(cacheDir, safeKey + ".json");
    }
}