using Puhu.Plugin;
using Termina.Layout;

namespace Puhu.Settings.Pages;

/// <summary>
/// Row builders shared by the Settings page sub-views and the setup wizard
/// steps, so theme/refresh lists render identically in both.
/// </summary>
internal static class SettingsRows
{
    public static IReadOnlyList<ILayoutNode> ThemeRows(
        IReadOnlyList<string> themes,
        int selectedIndex,
        string? savedTheme,
        ThemeDefinition theme)
    {
        var rows = new List<ILayoutNode>();

        for (var i = 0; i < themes.Count; i++)
        {
            var name = themes[i];
            var isSelected = i == selectedIndex;
            var isSaved = string.Equals(name, savedTheme, StringComparison.OrdinalIgnoreCase);
            var marker = isSelected ? "▸" : " ";
            var suffix = isSaved ? "  ●" : "";

            rows.Add(new TextNode($"{marker} {name}{suffix}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        if (themes.Count == 0)
        {
            rows.Add(new TextNode("no themes found").WithForeground(theme.TextDim).Height(1));
        }

        return rows;
    }

    public static IReadOnlyList<ILayoutNode> RefreshRows(
        IReadOnlyList<TimeSpan> steps,
        int selectedIndex,
        ThemeDefinition theme)
    {
        var rows = new List<ILayoutNode>();

        for (var i = 0; i < steps.Count; i++)
        {
            var isSelected = i == selectedIndex;
            var marker = isSelected ? "▸" : " ";
            rows.Add(new TextNode($"{marker} {IntervalFormat.Format(steps[i])}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        return rows;
    }
}
