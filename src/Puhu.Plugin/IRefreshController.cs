using R3;

namespace Puhu.Plugin;

/// <summary>
/// Controls the tick frequency of the shell's refresh loop.
/// Consumed by global key bindings, the top bar, and the settings page.
/// </summary>
public interface IRefreshController
{
    /// <summary>Current tick interval (always one of <see cref="Steps"/>).</summary>
    ReadOnlyReactiveProperty<TimeSpan> Interval { get; }

    /// <summary>Whether ticking is paused.</summary>
    ReadOnlyReactiveProperty<bool> IsPaused { get; }

    /// <summary>Available interval steps, fastest first.</summary>
    IReadOnlyList<TimeSpan> Steps { get; }

    /// <summary>Switch to the next faster step (no-op at the fastest).</summary>
    void SpeedUp();

    /// <summary>Switch to the next slower step (no-op at the slowest).</summary>
    void SlowDown();

    /// <summary>Toggle pause. Not persisted.</summary>
    void TogglePause();

    /// <summary>Set the interval, snapped to the nearest step. Persisted.</summary>
    void SetInterval(TimeSpan interval);
}

/// <summary>Formats refresh intervals for display (250ms, 500ms, 1s, 2s, 4s).</summary>
public static class IntervalFormat
{
    public static string Format(TimeSpan interval) =>
        interval < TimeSpan.FromSeconds(1)
            ? $"{(int)interval.TotalMilliseconds}ms"
            : $"{(int)interval.TotalSeconds}s";
}
