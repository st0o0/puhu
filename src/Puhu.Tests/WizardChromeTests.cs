using Puhu.Plugin;
using Puhu.Settings.Pages;
using Termina.Layout;

namespace Puhu.Tests;

public sealed class WizardChromeTests
{
    private static readonly ThemeDefinition Theme = new();

    [Fact]
    public void Breadcrumb_ShowsAllSteps_CurrentBracketed()
    {
        var output = Render(WizardChrome.Breadcrumb(SetupStep.Theme, Theme), 70);

        Assert.Contains("welcome", output);
        Assert.Contains("[theme]", output);
        Assert.Contains("done", output);
    }

    [Fact]
    public void StatusBar_ShowsDots_Count_AndHints()
    {
        var hints = WizardChrome.HintsFor(SetupStep.Theme);
        var output = Render(WizardChrome.StatusBar((int)SetupStep.Theme, WizardChrome.StepCount, hints, Theme), 70);

        Assert.Contains("●", output);
        Assert.Contains("step 2/6", output);
        Assert.Contains("choose", output);
    }

    [Fact]
    public void HintsFor_TabOrder_MentionsMove()
    {
        Assert.Contains("move", WizardChrome.HintsFor(SetupStep.TabOrder));
    }

    private static string Render(ILayoutNode node, int w)
    {
        var ctx = new RenderTestContext(w, 1);
        node.Measure(new Size(w, 1));
        node.Render(ctx, new Rect(0, 0, w, 1));
        return ctx.Row(0);
    }
}
