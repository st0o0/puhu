using System.Text.Json;
using Puhu.Marketplace.Models;

namespace Puhu.Marketplace.Tests;

public sealed class PluginManifestTests
{
    private const string ValidJson = """
        {
          "id": "servus.plugin.dtop",
          "name": "dtop",
          "description": "System monitoring",
          "author": "st0o0",
          "version": "1.2.0",
          "minPuhuVersion": "1.0.0",
          "license": "MIT",
          "repository": "https://github.com/st0o0/dtop",
          "delivery": { "type": "github-release", "asset": "Puhu.Plugin.Dtop.dll" },
          "tags": ["monitoring", "system"]
        }
        """;

    [Fact]
    public void Deserialize_ValidJson_ReturnsManifest()
    {
        var manifest = JsonSerializer.Deserialize<PluginManifest>(ValidJson)!;
        Assert.Equal("servus.plugin.dtop", manifest.Id);
        Assert.Equal("dtop", manifest.Name);
        Assert.Equal("1.2.0", manifest.Version);
        Assert.Equal(2, manifest.Tags.Count);
    }

    [Fact]
    public void Deserialize_GitHubReleaseDelivery()
    {
        var manifest = JsonSerializer.Deserialize<PluginManifest>(ValidJson)!;
        Assert.Equal(DeliveryType.GitHubRelease, manifest.Delivery.Type);
        Assert.Equal("Puhu.Plugin.Dtop.dll", manifest.Delivery.Asset);
        Assert.Null(manifest.Delivery.PackageId);
    }

    [Fact]
    public void Deserialize_NuGetDelivery()
    {
        var json = """
            {
              "id": "servus.plugin.k8s", "name": "Kubernetes", "description": "K8s",
              "author": "st0o0", "version": "0.1.0", "minPuhuVersion": "1.0.0",
              "license": "MIT", "repository": "https://github.com/st0o0/servus.plugin.k8s",
              "delivery": { "type": "nuget", "packageId": "Puhu.Plugin.K8s" },
              "tags": ["kubernetes"]
            }
            """;
        var manifest = JsonSerializer.Deserialize<PluginManifest>(json)!;
        Assert.Equal(DeliveryType.NuGet, manifest.Delivery.Type);
        Assert.Equal("Puhu.Plugin.K8s", manifest.Delivery.PackageId);
    }

    [Fact]
    public void Serialize_RoundTrips()
    {
        var manifest = JsonSerializer.Deserialize<PluginManifest>(ValidJson)!;
        var json = JsonSerializer.Serialize(manifest);
        var roundTripped = JsonSerializer.Deserialize<PluginManifest>(json)!;
        Assert.Equal(manifest.Id, roundTripped.Id);
        Assert.Equal(manifest.Delivery.Type, roundTripped.Delivery.Type);
    }
}
