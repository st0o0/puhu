using Puhu.Plugin;
using R3;

namespace Puhu.Tests.Fakes;

internal sealed class FakeRefreshController : IRefreshController
{
    private readonly ReactiveProperty<TimeSpan> _interval = new(TimeSpan.FromSeconds(1));
    private readonly ReactiveProperty<bool> _paused = new(false);

    public ReadOnlyReactiveProperty<TimeSpan> Interval => _interval;
    public ReadOnlyReactiveProperty<bool> IsPaused => _paused;
    public IReadOnlyList<TimeSpan> Steps { get; } =
    [
        TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(2000),
        TimeSpan.FromMilliseconds(4000),
    ];

    public void SpeedUp() { }
    public void SlowDown() { }
    public void TogglePause() => _paused.Value = !_paused.Value;
    public void SetInterval(TimeSpan interval) => _interval.Value = interval;
}
