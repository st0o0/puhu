using Puhu.Plugin;
using Termina.Layout;

namespace Puhu.Settings.Pages;

/// <summary>Content builders for the setup wizard steps. Kept out of the page so they're unit-testable.</summary>
internal static class WizardSteps
{
    /// <summary>
    /// Drops <paramref name="content"/> into the vertical middle of the wizard body using fill spacers,
    /// so a step sits centered instead of pinned to the top-left corner.
    /// </summary>
    public static ILayoutNode Centered(ILayoutNode content)
    {
        return Layouts.Vertical(
            new TextNode("").Fill(),
            content,
            new TextNode("").Fill());
    }

    /// <summary>
    /// Stacks <paramref name="rows"/> as a content-width block centered both vertically and horizontally.
    /// Rows are sized to their text so the block hugs its content, keeping list-style steps off the
    /// top-left corner while the row text itself stays left-aligned within the block.
    /// </summary>
    public static ILayoutNode CenteredRows(IReadOnlyList<ILayoutNode> rows)
    {
        foreach (var row in rows)
        {
            if (row is LayoutNode node)
            {
                node.WidthAuto();
            }
        }

        var block = Layouts.Horizontal(
            new TextNode("").WidthFill(),
            Layouts.Vertical(rows.ToArray()),
            new TextNode("").WidthFill());

        return Centered(block);
    }

    public static ILayoutNode Welcome(ThemeDefinition theme)
    {
        return Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode(PuhuBranding.LogoBlock).NoWrap().WithForeground(theme.Accent).AlignCenter(),
            new TextNode(""),
            new TextNode("A terminal dashboard, made yours.").WithForeground(theme.Foreground).AlignCenter(),
            new TextNode(""),
            new TextNode("Fast, keyboard-driven panels you extend with plugins,").WithForeground(theme.TextDim).AlignCenter(),
            new TextNode("all in one TUI. This quick setup gets you started.").WithForeground(theme.TextDim).AlignCenter(),
            new TextNode("").Fill());
    }

    public static ILayoutNode Done(ThemeDefinition theme, string themeName, string refreshLabel)
    {
        return Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode("✓  You're all set").WithForeground(theme.Success).AlignCenter(),
            new TextNode(""),
            new TextNode($"theme: {themeName} · refresh: {refreshLabel}").WithForeground(theme.TextDim).AlignCenter(),
            new TextNode("").Fill());
    }

    public static ILayoutNode Theme(
        IReadOnlyList<string> themes, int selectedIndex, string? savedTheme, ThemeDefinition theme)
    {
        return Centered(Layouts.Horizontal(
            Layouts.Vertical(SettingsRows.ThemeRows(themes, selectedIndex, savedTheme, theme, compact: true).ToArray()).Width(24),
            new ThemePaletteNode(theme)));
    }
}
