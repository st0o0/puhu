using Microsoft.Extensions.Time.Testing;
using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Tests.Fakes;
using Puhu.Tests.Helpers;
using R3;
using Termina.Layout;
using Termina.Terminal;

namespace Puhu.Tests.Nodes;

[Collection("TabRegistry")]
public sealed class TopBarNodeTests
{
    private static FakeTimeProvider CreateTime()
    {
        var tp = new FakeTimeProvider(new DateTimeOffset(2026, 6, 12, 12, 4, 33, TimeSpan.Zero));
        tp.SetLocalTimeZone(TimeZoneInfo.Utc);
        return tp;
    }

    private static VirtualTerminal Render(int width, string[] labels, int active, FakeRefreshController? controller = null)
    {
        TabRegistry.RegisterTabs(labels.Select(l => new PluginTabInfo(l, $"/{l}")).ToList());
        TabRegistry.CurrentTabIndex = active;

        var node = new TopBarNode(new FakeThemeService(), controller ?? new FakeRefreshController(), CreateTime());
        return Tui.Render(node, width, 1);
    }

    [Fact]
    public void Render_DrawsRoundedCornersAndLine()
    {
        var ctx = Render(60, ["Alpha"], 0);
        var row = ctx.Row(0);

        Assert.Equal('╭', row[0]);
        Assert.Equal('╮', row[^1]);
        Assert.Contains('─', row);
    }

    [Fact]
    public void Render_ShowsLogo()
    {
        var ctx = Render(60, ["Alpha"], 0);

        Assert.Contains("⏻ puhu", ctx.Row(0));
    }

    [Fact]
    public void Render_ShowsClock()
    {
        var ctx = Render(60, ["Alpha"], 0);

        Assert.Contains("12:04:33", ctx.Row(0));
    }

    [Fact]
    public void Render_ActiveTabIsBracketed_Lowercase()
    {
        var ctx = Render(60, ["Alpha", "Beta"], 0);

        Assert.Contains("┤ alpha ├", ctx.Row(0));
        Assert.Contains(" beta ", ctx.Row(0));
    }

    [Fact]
    public void Render_Overflow_ShowsArrows()
    {
        var labels = Enumerable.Range(1, 10).Select(i => $"plugin-{i:00}").ToArray();
        var ctx = Render(50, labels, 5);
        var row = ctx.Row(0);

        Assert.Contains('◀', row);
        Assert.Contains('▶', row);
        Assert.Contains("┤ plugin-06 ├", row);
    }

    [Fact]
    public void Render_TooNarrow_DoesNotThrow()
    {
        _ = Render(5, ["Alpha"], 0);
    }

    [Fact]
    public void Render_ClockNotOverwrittenByTabs_WhenSpaceTight()
    {
        // Width 30: logo + clock + one tab — tabs must not bleed into clock area
        var ctx = Render(30, ["Alpha"], 0);
        var row = ctx.Row(0);

        // Clock must still be present
        Assert.Contains("12:04:33", row);
        // Corner must be intact
        Assert.Equal('╮', row[^1]);
    }

    [Fact]
    public void Render_ShowsIntervalNextToClock()
    {
        var ctx = Render(60, ["Alpha"], 0); // default fake: 1s

        Assert.EndsWith("─1s─12:04:33─╮", ctx.Row(0));
    }

    [Fact]
    public void Render_Paused_ShowsPauseSymbolInsteadOfInterval()
    {
        var controller = new FakeRefreshController();
        controller.TogglePause();
        var ctx = Render(60, ["Alpha"], 0, controller);

        Assert.EndsWith("─⏸─12:04:33─╮", ctx.Row(0));
    }

    [Fact]
    public void Render_250ms_FormatsAsMilliseconds()
    {
        var controller = new FakeRefreshController();
        controller.SetInterval(TimeSpan.FromMilliseconds(250));
        var ctx = Render(60, ["Alpha"], 0, controller);

        Assert.EndsWith("─250ms─12:04:33─╮", ctx.Row(0));
    }
}
