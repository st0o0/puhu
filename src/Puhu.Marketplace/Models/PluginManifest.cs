using System.Text.Json.Serialization;

namespace Puhu.Plugin.Marketplace.Models;

public sealed record PluginManifest
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
    [JsonPropertyName("author")] public required string Author { get; init; }
    [JsonPropertyName("version")] public required string Version { get; init; }
    [JsonPropertyName("minServusVersion")] public required string MinServusVersion { get; init; }
    [JsonPropertyName("license")] public required string License { get; init; }
    [JsonPropertyName("repository")] public required string Repository { get; init; }
    [JsonPropertyName("delivery")] public required PluginDelivery Delivery { get; init; }
    [JsonPropertyName("tags")] public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record PluginDelivery
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<DeliveryType>))]
    public required DeliveryType Type { get; init; }
    [JsonPropertyName("asset")] public string? Asset { get; init; }
    [JsonPropertyName("packageId")] public string? PackageId { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<DeliveryType>))]
public enum DeliveryType
{
    [JsonStringEnumMemberName("github-release")] GitHubRelease,
    [JsonStringEnumMemberName("nuget")] NuGet
}
