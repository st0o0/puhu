using Puhu.Plugin;
using Puhu.Plugin.Nodes;
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

            var nameRow = new List<ILayoutNode>
            {
                new TextNode($"{marker} {name}")
                    .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                    .WidthFill()
            };

            if (isSaved)
            {
                nameRow.Add(new BadgeNode("ACTIVE", theme.SelectionText, theme.Success, icon: "●").Height(1));
            }

            rows.Add(Layouts.Horizontal(nameRow.ToArray()).Height(1));
            rows.Add(Layouts.Empty().Height(1));
        }

        if (themes.Count == 0)
            rows.Add(new TextNode("no themes found").WithForeground(theme.TextDim).Height(1));

        return rows;
    }

    public static IReadOnlyList<ILayoutNode> RefreshRows(
        IReadOnlyList<TimeSpan> steps,
        int selectedIndex,
        ThemeDefinition theme)
    {
        var descriptions = new Dictionary<int, string>
        {
            [250] = "Fastest — high CPU usage",
            [500] = "Recommended",
            [1000] = "Balanced",
            [2000] = "Low power",
            [4000] = "Slowest — minimal resources",
        };

        var rows = new List<ILayoutNode>();
        for (var i = 0; i < steps.Count; i++)
        {
            var ms = (int)steps[i].TotalMilliseconds;
            var isSelected = i == selectedIndex;
            var marker = isSelected ? "▸" : " ";
            var label = IntervalFormat.Format(steps[i]);
            var desc = descriptions.GetValueOrDefault(ms, "");
            var descColor = ms == 500 ? theme.Success : theme.TextDim;

            rows.Add(Layouts.Horizontal(
                new TextNode($"{marker} {label}").WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthAuto(min: 10),
                new TextNode(desc).WithForeground(descColor).WidthFill()
            ).Height(1));
        }

        return rows;
    }
}
