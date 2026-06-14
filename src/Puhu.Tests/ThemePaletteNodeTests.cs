using Puhu.Plugin;
using Puhu.Settings.Pages;
using Termina.Layout;
using Termina.Terminal;

namespace Puhu.Tests;

public sealed class ThemePaletteNodeTests
{
    [Fact]
    public void Renders_RoleNames_Hex_AndGradient()
    {
        var theme = new ThemeDefinition
        {
            Background = Color.FromHex("#0a0e14"),
            Accent = Color.FromHex("#e3b341"),
        };
        var node = new ThemePaletteNode(theme);

        var output = Render(node, 40, 20);

        Assert.Contains("background", output);
        Assert.Contains("#0a0e14", output);
        Assert.Contains("accent", output);
        Assert.Contains("graph gradient", output);
    }

    private static string Render(ILayoutNode node, int w, int h)
    {
        var ctx = new RenderTestContext(w, h);
        node.Measure(new Size(w, h));
        node.Render(ctx, new Rect(0, 0, w, h));
        var rows = new string[h];
        for (var y = 0; y < h; y++)
        {
            rows[y] = ctx.Row(y);
        }
        return string.Join('\n', rows);
    }
}
