using Puhu.Plugin;
using R3;
using Termina.Input;

namespace Puhu.Plugin.Tests;

public sealed class GlobalKeyExtensionsTests
{
    [Fact]
    public void RegisterGlobalKeys_WithController_BindsSpeedKeys()
    {
        var keys = new PageKeyBindings();
        var controller = new FakeRefreshController();

        keys.RegisterGlobalKeys(() => { }, _ => { }, new FakeTabNavigator(), controller);

        keys.TryHandle(new ConsoleKeyInfo('+', ConsoleKey.OemPlus, false, false, false));
        Assert.Equal(1, controller.SpeedUps);

        keys.TryHandle(new ConsoleKeyInfo('-', ConsoleKey.OemMinus, false, false, false));
        Assert.Equal(1, controller.SlowDowns);

        keys.TryHandle(new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false));
        Assert.Equal(1, controller.PauseToggles);
    }

    [Fact]
    public void RegisterGlobalKeys_WithController_BindsNumpadKeys()
    {
        var keys = new PageKeyBindings();
        var controller = new FakeRefreshController();

        keys.RegisterGlobalKeys(() => { }, _ => { }, new FakeTabNavigator(), controller);

        keys.TryHandle(new ConsoleKeyInfo('+', ConsoleKey.Add, false, false, false));
        keys.TryHandle(new ConsoleKeyInfo('-', ConsoleKey.Subtract, false, false, false));

        Assert.Equal(1, controller.SpeedUps);
        Assert.Equal(1, controller.SlowDowns);
    }

    private sealed class FakeTabNavigator : ITabNavigator
    {
        public bool HasTabs => false;
        public void CycleTab(Action<string> navigate, int delta) { }
    }

    internal sealed class FakeRefreshController : IRefreshController
    {
        private readonly ReactiveProperty<TimeSpan> _interval = new(TimeSpan.FromSeconds(1));
        private readonly ReactiveProperty<bool> _paused = new(false);

        public int SpeedUps { get; private set; }
        public int SlowDowns { get; private set; }
        public int PauseToggles { get; private set; }

        public ReadOnlyReactiveProperty<TimeSpan> Interval => _interval;
        public ReadOnlyReactiveProperty<bool> IsPaused => _paused;
        public IReadOnlyList<TimeSpan> Steps { get; } =
        [
            TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(2000),
            TimeSpan.FromMilliseconds(4000),
        ];

        public void SpeedUp() => SpeedUps++;
        public void SlowDown() => SlowDowns++;
        public void TogglePause()
        {
            PauseToggles++;
            _paused.Value = !_paused.Value;
        }
        public void SetInterval(TimeSpan interval) => _interval.Value = interval;
    }
}
