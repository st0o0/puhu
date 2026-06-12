using Puhu.Marketplace.Models;

namespace Puhu.Marketplace;

/// <summary>Immutable snapshot of the marketplace state, pushed from actor to store.</summary>
public sealed record MarketplaceState
{
    public IReadOnlyList<PluginInfo> AvailablePlugins { get; init; } = [];
    public bool IsBusy { get; init; }
    public string? StatusMessage { get; init; }
    public IReadOnlyDictionary<string, string> ActiveOperations { get; init; } =
        new Dictionary<string, string>();
    public PluginSources Sources { get; init; } = new();
}
