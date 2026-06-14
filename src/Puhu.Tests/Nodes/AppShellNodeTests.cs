using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Tests.Fakes;
using Puhu.Tests.Helpers;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Tests.Nodes;

[Collection("TabRegistry")]
public sealed class AppShellNodeTests
{
    private static VirtualTerminal Render(int w, int h, ILayoutNode content, params string[] hints)
    {
        TabRegistry.RegisterTabs([new PluginTabInfo("Alpha", "/alpha")]);
        var node = new AppShellNode(new FakeThemeService(), new FakeRefreshController(), content, hints);
        return Tui.Render(node, w, h);
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

        Assert.True(ctx.IsBlank());
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
}
