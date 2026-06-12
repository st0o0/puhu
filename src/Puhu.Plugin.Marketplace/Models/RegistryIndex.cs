using System.Text.Json.Serialization;

namespace Puhu.Plugin.Marketplace.Models;

public sealed record RegistryIndex
{
    [JsonPropertyName("version")] public required int Version { get; init; }
    [JsonPropertyName("plugins")] public required IReadOnlyList<RegistryEntry> Plugins { get; init; }
}

public sealed record RegistryEntry
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("repository")] public required string Repository { get; init; }
}
