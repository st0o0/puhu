using System.Text.Json.Serialization;

namespace Servus.Plugin.Marketplace.Models;

public sealed record PluginSources
{
    [JsonPropertyName("registries")] public List<string> Registries { get; init; } = [];
    [JsonPropertyName("repositories")] public List<string> Repositories { get; init; } = [];
}
