using Puhu.Nodes;

namespace Puhu.Tests.Nodes;

public sealed class TabScrollWindowTests
{
    [Fact]
    public void AllTabsFit_NoArrows()
    {
        // 3 tabs à 5 chars: active 9 + 2×7 + 2 separators = 25
        var window = TabScrollWindow.Compute([5, 5, 5], activeIndex: 0, available: 30);

        Assert.Equal((0, 2, false, false), (window.First, window.Last, window.HasLeft, window.HasRight));
    }

    [Fact]
    public void Overflow_ActiveAlwaysVisible()
    {
        var lengths = Enumerable.Repeat(8, 10).ToArray();

        var window = TabScrollWindow.Compute(lengths, activeIndex: 9, available: 30);

        Assert.True(window.First <= 9 && 9 <= window.Last);
        Assert.True(window.HasLeft);
        Assert.False(window.HasRight);
    }

    [Fact]
    public void Overflow_MiddleActive_BothArrows()
    {
        var lengths = Enumerable.Repeat(8, 10).ToArray();

        var window = TabScrollWindow.Compute(lengths, activeIndex: 5, available: 30);

        Assert.True(window.HasLeft);
        Assert.True(window.HasRight);
        Assert.True(window.First <= 5 && 5 <= window.Last);
    }

    [Fact]
    public void BarelyFitsActive_WindowIsActiveOnly()
    {
        var window = TabScrollWindow.Compute([10, 10, 10], activeIndex: 1, available: 18);

        Assert.Equal(1, window.First);
        Assert.Equal(1, window.Last);
    }

    [Fact]
    public void NoTabs_EmptyWindow()
    {
        var window = TabScrollWindow.Compute([], activeIndex: 0, available: 30);

        Assert.True(window.Last < window.First);
    }
}
