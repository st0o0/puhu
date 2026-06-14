using Puhu.Plugin;
using Termina.Layout;

namespace Puhu.Settings.Pages;

/// <summary>Content builders for the setup wizard steps. Kept out of the page so they're unit-testable.</summary>
internal static class WizardSteps
{
    public static ILayoutNode Welcome(ThemeDefinition theme)
    {
        return Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode(PuhuBranding.Logo).WithForeground(theme.Accent).AlignCenter(),
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
        return Layouts.Horizontal(
            Layouts.Vertical(SettingsRows.ThemeRows(themes, selectedIndex, savedTheme, theme).ToArray()).Width(24),
            new ThemePaletteNode(theme));
    }
}
