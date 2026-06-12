using Termina.Layout;

namespace Puhu.Tests;

public sealed class RenderTestContextTests
{
    [Fact]
    public void WriteAt_ThenRow_RoundTrips()
    {
        var ctx = new RenderTestContext(10, 2);
        ctx.WriteAt(1, 0, "abc");

        Assert.Equal(" abc      ", ctx.Row(0));
    }

    [Fact]
    public void SubContext_OffsetsWrites()
    {
        var ctx = new RenderTestContext(10, 3);
        var sub = ctx.CreateSubContext(new Rect(2, 1, 5, 2));
        sub.WriteAt(0, 0, "xy");

        Assert.Equal("  xy      ", ctx.Row(1));
    }

    [Fact]
    public void SubContext_ClipsOutOfBounds()
    {
        var ctx = new RenderTestContext(10, 2);
        var sub = ctx.CreateSubContext(new Rect(8, 0, 2, 1));
        sub.WriteAt(0, 0, "abcdef");

        Assert.Equal("        ab", ctx.Row(0));
    }

    [Fact]
    public void SubContext_ExceedingRoot_IsClippedWithoutThrowing()
    {
        var ctx = new RenderTestContext(10, 1);
        var sub = ctx.CreateSubContext(new Rect(8, 0, 5, 1));
        sub.WriteAt(0, 0, "abcde");

        Assert.Equal("        ab", ctx.Row(0));
    }

    [Fact]
    public void NestedSubContext_ClipsAgainstParent()
    {
        var ctx = new RenderTestContext(10, 2);
        var outer = ctx.CreateSubContext(new Rect(2, 0, 6, 2));
        var inner = outer.CreateSubContext(new Rect(4, 0, 10, 1));
        inner.WriteAt(0, 0, "zzzz");

        Assert.Equal("      zz  ", ctx.Row(0));
    }
}
