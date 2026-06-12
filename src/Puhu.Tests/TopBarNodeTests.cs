using Microsoft.Extensions.Time.Testing;
using Puhu.Nodes;
using Puhu.Plugin;
using R3;
using Termina.Layout;

namespace Puhu.Tests;

[Collection("TabRegistry")]
public sealed class TopBarNodeTests
{
    private static FakeTimeProvider CreateTime()
    {
        var tp = new FakeTimeProvider(new DateTimeOffset(2026, 6, 12, 12, 4, 33, TimeSpan.Zero));
        tp.SetLocalTimeZone(TimeZoneInfo.Utc);
        return tp;
    }

    private static RenderTestContext Render(int width, string[] labels, int active)
    {
        TabRegistry.RegisterTabs(labels.Select(l => new PluginTabInfo(l, $"/{l}")).ToList());
        TabRegistry.CurrentTabIndex = active;

        var node = new TopBarNode(new FakeThemeService(), CreateTime());
        var ctx = new RenderTestContext(width, 1);
        node.Render(ctx, new Rect(0, 0, width, 1));
        return ctx;
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

    private sealed class FakeThemeService : IThemeService
    {
        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName => null;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => [];
        public bool ApplyByName(string name) => false;
        public void SaveCurrent() { }
    }
}
