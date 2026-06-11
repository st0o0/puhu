using System.Text.Json.Serialization;

namespace Servus.Plugin.Marketplace.Models;

public sealed record InstalledPluginsFile
{
    [JsonPropertyName("plugins")] public List<InstalledPlugin> Plugins { get; init; } = [];
}

public sealed record InstalledPlugin
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("version")] public required string Version { get; init; }
    [JsonPropertyName("source")] public required string Source { get; init; }
    [JsonPropertyName("updatePolicy")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdatePolicy>))]
    public UpdatePolicy UpdatePolicy { get; set; }
    [JsonPropertyName("path")] public required string Path { get; init; }
}

public enum UpdatePolicy { Auto, Manual, Pinned }
