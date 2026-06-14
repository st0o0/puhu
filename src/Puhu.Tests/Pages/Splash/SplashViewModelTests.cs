using Microsoft.Extensions.Time.Testing;
using Puhu.Pages;
using Puhu.Setup;

namespace Puhu.Tests.Pages.Splash;

public sealed class SplashViewModelTests : IDisposable
{
    private readonly FakeTimeProvider _time = new();
    private readonly SplashViewModel _vm;

    public SplashViewModelTests()
    {
        _vm = new SplashViewModel(new StartPageRoute("/marketplace"), _time);
    }

    [Fact]
    public void Progress_StartsAtZero()
    {
        Assert.Equal(0.0, _vm.Progress.Value);
    }

    [Fact]
    public void Progress_AnimatesToCompletion()
    {
        _vm.OnActivated();

        for (var i = 0; i < 20; i++)
        {
            _time.Advance(TimeSpan.FromMilliseconds(50));
        }

        Assert.Equal(1.0, _vm.Progress.Value);
    }

    [Fact]
    public void Navigate_CalledAfterProgressCompletes()
    {
        _vm.OnActivated();

        for (var i = 0; i < 20; i++)
        {
            _time.Advance(TimeSpan.FromMilliseconds(50));
        }

        Assert.Equal(1.0, _vm.Progress.Value);
        Assert.Equal("Starting...", _vm.StatusText.Value);
    }

    public void Dispose() => _vm.Dispose();
}
