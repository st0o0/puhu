using Puhu.Plugin;
using Puhu.Services;
using Puhu.Themes;
using R3;

namespace Puhu.Tests;

public sealed class ShellRedrawServiceTests
{
    [Fact]
    public async Task Tick_TriggersRedraw()
    {
        var ticks = new Subject<Tick>();
        var themeService = new ThemeService();
        var redraws = 0;
        var service = new ShellRedrawService(new FakeTickSource(ticks), themeService, () => redraws++);

        await service.StartAsync(CancellationToken.None);
        ticks.OnNext(new Tick(1, TimeSpan.FromSeconds(1)));

        Assert.Equal(1, redraws);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ThemeChange_TriggersRedraw()
    {
        var themeService = new ThemeService();
        var redraws = 0;
        var service = new ShellRedrawService(new FakeTickSource(new Subject<Tick>()), themeService, () => redraws++);

        await service.StartAsync(CancellationToken.None);
        themeService.Apply(new ThemeDefinition());

        Assert.Equal(1, redraws);
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task AfterStop_NoMoreRedraws()
    {
        var ticks = new Subject<Tick>();
        var service = new ShellRedrawService(new FakeTickSource(ticks), new ThemeService(), () => Assert.Fail("redraw after stop"));

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);
        ticks.OnNext(new Tick(1, TimeSpan.FromSeconds(1)));
    }

    private sealed class FakeTickSource(Subject<Tick> ticks) : ITickSource
    {
        public TimeSpan CurrentInterval => TimeSpan.FromSeconds(1);
        public Observable<Tick> Ticks => ticks;
        public IDisposable Subscribe(Action onTick) => ticks.Subscribe(_ => onTick());
    }
}
