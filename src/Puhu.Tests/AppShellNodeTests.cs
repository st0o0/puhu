using Puhu.Nodes;
using Puhu.Plugin;
using R3;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Tests;

[Collection("TabRegistry")]
public sealed class AppShellNodeTests
{
    private static RenderTestContext Render(int w, int h, ILayoutNode content, params string[] hints)
    {
        TabRegistry.RegisterTabs([new PluginTabInfo("Alpha", "/alpha")]);
        var node = new AppShellNode(new FakeThemeService(), content, hints);
        var ctx = new RenderTestContext(w, h);
        node.Render(ctx, new Rect(0, 0, w, h));
        return ctx;
    }

    [Fact]
    public void Render_DrawsBottomCorners()
    {
        var ctx = Render(40, 10, new EmptyNode());
        var bottom = ctx.Row(9);

        Assert.Equal('╰', bottom[0]);
        Assert.Equal('╯', bottom[^1]);
    }

    [Fact]
    public void Render_DrawsSideBorders()
    {
        var ctx = Render(40, 10, new EmptyNode());

        for (var y = 1; y < 9; y++)
        {
            Assert.Equal('│', ctx.Row(y)[0]);
            Assert.Equal('│', ctx.Row(y)[^1]);
        }
    }

    [Fact]
    public void Render_EmbedsKeyHintsInBottomBorder()
    {
        var ctx = Render(40, 10, new EmptyNode(), "Esc:Quit", "Tab:Switch");
        var bottom = ctx.Row(9);

        Assert.Contains("┤ Esc quit ├", bottom);
        Assert.Contains("┤ Tab switch ├", bottom);
    }

    [Fact]
    public void Render_KeyOnlyHint_RendersCleanSegment()
    {
        var ctx = Render(40, 10, new EmptyNode(), "Esc");
        var bottom = ctx.Row(9);

        Assert.Contains("┤ Esc ├", bottom);
    }

    [Fact]
    public void Render_ContentGetsInnerBounds()
    {
        var probe = new BoundsProbeNode();
        Render(40, 10, probe);

        Assert.Equal(new Rect(1, 1, 38, 8), probe.LastBounds);
    }

    [Fact]
    public void Render_TooSmall_RendersNothing()
    {
        var ctx = Render(6, 2, new EmptyNode());

        Assert.Empty(ctx.Writes);
    }

    [Fact]
    public void Render_HintsExceedingWidth_AreTruncatedNotOverflowing()
    {
        var ctx = Render(24, 5, new EmptyNode(), "Esc:Quit", "Tab:Switch", "Enter:Confirm");
        var bottom = ctx.Row(4);

        Assert.Equal('╰', bottom[0]);
        Assert.Equal('╯', bottom[^1]);
    }

    private sealed class BoundsProbeNode : LayoutNode
    {
        public Rect LastBounds { get; private set; }
        public override Size Measure(Size available) => available;
        public override void Render(IRenderContext context, Rect bounds) => LastBounds = bounds;
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
