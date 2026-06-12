using R3;

namespace Puhu.Plugin.Marketplace;

/// <summary>
/// Reactive read-only bridge between MarketplaceActor and the ViewModel.
/// The actor pushes state snapshots; the ViewModel subscribes to observables.
/// </summary>
public sealed class MarketplaceStore : IDisposable
{
    private readonly ReactiveProperty<MarketplaceState> _state = new(new MarketplaceState());

    /// <summary>Observable stream of marketplace state changes.</summary>
    public Observable<MarketplaceState> State => _state;

    /// <summary>Current state snapshot for synchronous access.</summary>
    public MarketplaceState Current => _state.Value;

    public void Update(MarketplaceState state) => _state.Value = state;

    public void Dispose() => _state.Dispose();
}
