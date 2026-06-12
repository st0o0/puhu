using R3;
using Puhu.Plugin;

namespace Puhu.Services;

public sealed class RefreshService : ITickSource, IRefreshController, IDisposable
{
    private static readonly TimeSpan[] StepsTable =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(1000),
        TimeSpan.FromMilliseconds(2000),
        TimeSpan.FromMilliseconds(4000),
    ];

    private const string IntervalSettingKey = "puhu.refresh-interval";

    private readonly Subject<Tick> _ticks = new();
    private readonly Lock _gate = new();
    private readonly ISettingsStore? _settings;
    private IDisposable? _timer;
    private long _seq;

    public ReactiveProperty<TimeSpan> Interval { get; }
    public ReactiveProperty<bool> IsPaused { get; } = new(false);
    public Observable<Tick> Ticks => _ticks.AsObservable();
    public TimeSpan CurrentInterval => Interval.Value;

    public RefreshService(TimeSpan initialInterval, ISettingsStore? settings = null)
    {
        _settings = settings;
        Interval = new ReactiveProperty<TimeSpan>(SnapToStep(initialInterval));
        StartTimer();
    }

    public void SpeedUp() => Shift(-1);
    public void SlowDown() => Shift(+1);

    ReadOnlyReactiveProperty<TimeSpan> IRefreshController.Interval => Interval;
    ReadOnlyReactiveProperty<bool> IRefreshController.IsPaused => IsPaused;

    public IReadOnlyList<TimeSpan> Steps => StepsTable;

    public void TogglePause() => IsPaused.Value = !IsPaused.Value;

    public void SetInterval(TimeSpan interval)
    {
        lock (_gate)
        {
            var snapped = SnapToStep(interval);
            if (snapped == Interval.Value)
            {
                return;
            }

            Interval.Value = snapped;
            PersistInterval();
            StartTimerCore();
        }
    }

    private void PersistInterval() =>
        _settings?.Set(IntervalSettingKey, (int)Interval.Value.TotalMilliseconds);

    private void Shift(int direction)
    {
        lock (_gate)
        {
            var idx = Array.IndexOf(StepsTable, Interval.Value);
            var next = Math.Clamp(idx + direction, 0, StepsTable.Length - 1);
            if (idx < 0 || StepsTable[next] == Interval.Value)
            {
                return;
            }

            Interval.Value = StepsTable[next];
            PersistInterval();
            StartTimerCore();
        }
    }

    private void StartTimer()
    {
        lock (_gate)
        {
            StartTimerCore();
        }
    }

    private void StartTimerCore()
    {
        _timer?.Dispose();
        _timer = Observable.Interval(Interval.Value, TimeProvider.System)
            .Subscribe(_ =>
            {
                if (IsPaused.Value)
                {
                    return;
                }

                _ticks.OnNext(new Tick(Interlocked.Increment(ref _seq) - 1, Interval.Value));
            });
    }

    private static TimeSpan SnapToStep(TimeSpan value) => StepsTable.MinBy(s => Math.Abs((s - value).Ticks));

    IDisposable ITickSource.Subscribe(Action onTick) => Ticks.Subscribe(_ => onTick());

    public void Dispose()
    {
        lock (_gate)
        {
            _timer?.Dispose();
        }

        _ticks.OnCompleted();
        _ticks.Dispose();
        Interval.Dispose();
        IsPaused.Dispose();
    }
}
