using Servus.TUI.Services;
using Servus.Plugin;

namespace Servus.TUI.Tests;

public sealed class RefreshServiceTests : IDisposable
{
    private readonly RefreshService _sut;

    public RefreshServiceTests()
    {
        _sut = new RefreshService(TimeSpan.FromMilliseconds(1000));
    }

    public void Dispose() => _sut.Dispose();

    [Fact]
    public void CurrentInterval_ReturnsSnappedInterval()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(1000), _sut.CurrentInterval);
    }

    [Fact]
    public void CurrentInterval_SnapsToNearestStep()
    {
        using var sut = new RefreshService(TimeSpan.FromMilliseconds(600));
        Assert.Equal(TimeSpan.FromMilliseconds(500), sut.CurrentInterval);
    }

    [Fact]
    public void SpeedUp_DecreasesInterval()
    {
        _sut.SpeedUp();
        Assert.Equal(TimeSpan.FromMilliseconds(500), _sut.CurrentInterval);
    }

    [Fact]
    public void SlowDown_IncreasesInterval()
    {
        _sut.SlowDown();
        Assert.Equal(TimeSpan.FromMilliseconds(2000), _sut.CurrentInterval);
    }

    [Fact]
    public void SpeedUp_AtMinimum_StaysAtMinimum()
    {
        using var sut = new RefreshService(TimeSpan.FromMilliseconds(250));
        sut.SpeedUp();
        Assert.Equal(TimeSpan.FromMilliseconds(250), sut.CurrentInterval);
    }

    [Fact]
    public void SlowDown_AtMaximum_StaysAtMaximum()
    {
        using var sut = new RefreshService(TimeSpan.FromMilliseconds(4000));
        sut.SlowDown();
        Assert.Equal(TimeSpan.FromMilliseconds(4000), sut.CurrentInterval);
    }

    [Fact]
    public void Subscribe_ReceivesTicks()
    {
        var received = 0;
        using var sub = ((ITickSource)_sut).Subscribe(() => Interlocked.Increment(ref received));
        Thread.Sleep(1200);
        Assert.True(received > 0);
    }
}
