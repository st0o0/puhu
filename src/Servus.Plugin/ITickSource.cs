using R3;

namespace Servus.Plugin;

/// <summary>
/// Provides periodic tick events for UI refresh and actor scheduling.
/// </summary>
public interface ITickSource
{
    /// <summary>The current interval between ticks.</summary>
    TimeSpan CurrentInterval { get; }

    /// <summary>Observable stream of ticks — use R3 operators to filter, throttle, or combine.</summary>
    Observable<Tick> Ticks { get; }

    /// <summary>Subscribe to tick events with a simple callback. Returns a disposable to unsubscribe.</summary>
    IDisposable Subscribe(Action onTick);
}
