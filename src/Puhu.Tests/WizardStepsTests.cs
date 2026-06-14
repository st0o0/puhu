using Puhu.Plugin;
using Puhu.Settings.Pages;
using Termina.Layout;
using Termina.Terminal;

namespace Puhu.Tests;

public sealed class WizardStepsTests
{
    [Fact]
    public void Theme_RendersNamesAndPalette()
    {
        var theme = new ThemeDefinition { Background = Color.FromHex("#0a0e14") };
        var node = WizardSteps.Theme(["btop-default", "tokyo-night"], 0, "btop-default", theme);

        var output = Render(node, 60, 18);

        Assert.Contains("btop-default", output); // name list
        Assert.Contains("background", output);    // palette role
        Assert.Contains("#0a0e14", output);        // hex value
    }

    [Fact]
    public void Welcome_DescribesPuhu()
    {
        var output = Render(WizardSteps.Welcome(new ThemeDefinition()), 60, 14);

        Assert.Contains("terminal dashboard", output);
    }

    [Fact]
    public void Done_ShowsSummary()
    {
        var output = Render(WizardSteps.Done(new ThemeDefinition(), "btop-default", "1s"), 60, 8);

        Assert.Contains("You're all set", output);
        Assert.Contains("theme: btop-default", output);
        Assert.Contains("refresh: 1s", output);
    }

    internal static string Render(ILayoutNode node, int w, int h)
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
