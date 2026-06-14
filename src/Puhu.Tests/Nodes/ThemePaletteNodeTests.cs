using Puhu.Plugin;
using Puhu.Settings.Pages;
using Puhu.Tests.Helpers;
using Termina.Layout;
using Termina.Terminal;

namespace Puhu.Tests.Nodes;

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

    private static string Render(ILayoutNode node, int w, int h) => Tui.Render(node, w, h).Snapshot();
}
