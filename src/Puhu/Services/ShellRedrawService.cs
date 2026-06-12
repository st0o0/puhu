using Microsoft.Extensions.Hosting;
using Puhu.Plugin;
using R3;

namespace Puhu.Services;

/// <summary>
/// Triggers app-wide redraws: on every tick (live clock in the top bar)
/// and on theme changes (live switching without page interaction).
/// </summary>
internal sealed class ShellRedrawService(
    ITickSource tickSource,
    IThemeService themeService,
    Action requestRedraw) : IHostedService, IDisposable
{
    private IDisposable? _subscriptions;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tickSub = tickSource.Ticks.Subscribe(_ => requestRedraw());
        var themeSub = themeService.Changes.Subscribe(_ => requestRedraw());
        _subscriptions = Disposable.Combine(tickSub, themeSub);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
        _subscriptions = null;
    }
}
