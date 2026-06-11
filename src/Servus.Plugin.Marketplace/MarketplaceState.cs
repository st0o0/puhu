using Servus.Plugin.Marketplace.Models;

namespace Servus.Plugin.Marketplace;

/// <summary>Immutable snapshot of the marketplace state, pushed from actor to store.</summary>
public sealed record MarketplaceState
{
    public IReadOnlyList<PluginInfo> AvailablePlugins { get; init; } = [];
    public bool IsBusy { get; init; }
    public string? StatusMessage { get; init; }
}
