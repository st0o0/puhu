using R3;
using Puhu.Setup;
using Termina.Reactive;

namespace Puhu.Pages;

public sealed class SplashViewModel : ReactiveViewModel
{
    private readonly StartPageRoute _startPage;
    private readonly TimeProvider _timeProvider;
    private IDisposable? _animation;

    public ReactiveProperty<double> Progress { get; } = new(0.0);
    public ReactiveProperty<string> StatusText { get; } = new("Starting...");

    public SplashViewModel(StartPageRoute startPage, TimeProvider? timeProvider = null)
    {
        _startPage = startPage;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public override void OnActivated()
    {
        const int steps = 20;
        const int intervalMs = 50;
        var current = 0;

        _animation = Observable.Interval(TimeSpan.FromMilliseconds(intervalMs), _timeProvider)
            .Subscribe(_ =>
            {
                current++;
                Progress.Value = Math.Min(1.0, (double)current / steps);

                if (current >= steps)
                {
                    _animation?.Dispose();
                    _animation = null;
                    Navigate(_startPage.Route);
                }
            });
    }

    public override void OnDeactivating()
    {
        _animation?.Dispose();
        _animation = null;
        base.OnDeactivating();
    }

    public override void Dispose()
    {
        _animation?.Dispose();
        Progress.Dispose();
        StatusText.Dispose();
        base.Dispose();
    }
}
