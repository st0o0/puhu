namespace Puhu.Plugin.Marketplace.Models;

public sealed record PluginInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Author { get; init; }
    public required string AvailableVersion { get; init; }
    public required string Repository { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required PluginDelivery Delivery { get; init; }
    public string? InstalledVersion { get; init; }
    public UpdatePolicy? UpdatePolicy { get; init; }
    public PluginStatus Status { get; init; }

    public static PluginInfo FromManifest(PluginManifest manifest, string? installedVersion, UpdatePolicy? updatePolicy)
    {
        var status = installedVersion switch
        {
            null => PluginStatus.NotInstalled,
            _ when Version.Parse(manifest.Version) > Version.Parse(installedVersion) => PluginStatus.UpdateAvailable,
            _ => PluginStatus.Installed
        };
        return new PluginInfo
        {
            Id = manifest.Id, Name = manifest.Name, Description = manifest.Description,
            Author = manifest.Author, AvailableVersion = manifest.Version, Repository = manifest.Repository,
            Tags = manifest.Tags, Delivery = manifest.Delivery, InstalledVersion = installedVersion,
            UpdatePolicy = updatePolicy, Status = status
        };
    }
}

public enum PluginStatus { NotInstalled, Installed, UpdateAvailable }
