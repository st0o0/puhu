using Puhu.Plugin.Marketplace.Models;

namespace Puhu.Marketplace.Tests;

public sealed class PluginInfoTests
{
    [Fact]
    public void FromManifest_NotInstalled()
    {
        var manifest = CreateManifest("servus.plugin.dtop", "1.0.0");
        var info = PluginInfo.FromManifest(manifest, installedVersion: null, updatePolicy: null);
        Assert.Equal(PluginStatus.NotInstalled, info.Status);
        Assert.Null(info.InstalledVersion);
    }

    [Fact]
    public void FromManifest_Installed_SameVersion()
    {
        var manifest = CreateManifest("servus.plugin.dtop", "1.0.0");
        var info = PluginInfo.FromManifest(manifest, installedVersion: "1.0.0", updatePolicy: UpdatePolicy.Auto);
        Assert.Equal(PluginStatus.Installed, info.Status);
    }

    [Fact]
    public void FromManifest_UpdateAvailable()
    {
        var manifest = CreateManifest("servus.plugin.dtop", "1.1.0");
        var info = PluginInfo.FromManifest(manifest, installedVersion: "1.0.0", updatePolicy: UpdatePolicy.Auto);
        Assert.Equal(PluginStatus.UpdateAvailable, info.Status);
        Assert.Equal("1.1.0", info.AvailableVersion);
    }

    private static PluginManifest CreateManifest(string id, string version) => new()
    {
        Id = id, Name = id, Description = "Test", Author = "test", Version = version,
        MinServusVersion = "1.0.0", License = "MIT",
        Repository = $"https://github.com/test/{id}",
        Delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = $"{id}.dll" }
    };
}
