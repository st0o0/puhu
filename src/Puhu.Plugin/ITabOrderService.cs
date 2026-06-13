using R3;

namespace Puhu.Plugin;

/// <summary>
/// Exposes the current tab order and lets callers (the Settings UI, the setup
/// wizard) reorder tabs. Implemented by the runner; the persisted order survives
/// restarts.
/// </summary>
public interface ITabOrderService
{
    /// <summary>The tabs in current display order, left to right.</summary>
    IReadOnlyList<TabDescriptor> Tabs { get; }

    /// <summary>Emits after any reorder so UI can re-render.</summary>
    Observable<Unit> Changed { get; }

    /// <summary>
    /// Move the tab at <paramref name="index"/> by <paramref name="delta"/>
    /// positions (clamped). Persists the new order and notifies observers.
    /// </summary>
    void Move(int index, int delta);
}
