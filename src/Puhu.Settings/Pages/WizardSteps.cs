using Puhu.Plugin;
using Termina.Layout;

namespace Puhu.Settings.Pages;

/// <summary>Content builders for the setup wizard steps. Kept out of the page so they're unit-testable.</summary>
internal static class WizardSteps
{
    public static ILayoutNode Theme(
        IReadOnlyList<string> themes, int selectedIndex, string? savedTheme, ThemeDefinition theme)
    {
        return Layouts.Horizontal(
            Layouts.Vertical(SettingsRows.ThemeRows(themes, selectedIndex, savedTheme, theme).ToArray()).Width(24),
            new ThemePaletteNode(theme));
    }
}
