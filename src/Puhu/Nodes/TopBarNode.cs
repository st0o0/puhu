using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

/// <summary>
/// Top frame row of the shell: ╭─⏻ puhu─◀─ tab ─┤ active ├─ tab ─▶──HH:mm:ss─╮
/// </summary>
internal sealed class TopBarNode : LayoutNode
{
    private const string Logo = "⏻ puhu";

    private readonly IThemeService _themeService;
    private readonly IRefreshController _refreshController;
    private readonly TimeProvider _timeProvider;

    public TopBarNode(IThemeService themeService, IRefreshController refreshController, TimeProvider? timeProvider = null)
    {
        _themeService = themeService;
        _refreshController = refreshController;
        _timeProvider = timeProvider ?? TimeProvider.System;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea || bounds.Width < 12)
        {
            return;
        }

        var theme = _themeService.Current;
        var ctx = context.CreateSubContext(bounds);
        var w = bounds.Width;

        // 1. Fill with border line, then place corners
        ctx.SetForeground(theme.Border);
        ctx.Fill(0, 0, w, 1, '─');
        ctx.WriteAt(0, 0, '╭');
        ctx.WriteAt(w - 1, 0, '╮');

        // 2. Logo – fixed at x=2
        ctx.SetForeground(theme.Success);
        ctx.WriteAt(2, 0, Logo);

        // tabsStart: one '─' separator after logo
        var tabsStart = 2 + Logo.Length + 1;

        // 3. Clock and interval — written near the right edge
        // layout: ...─{interval}─{clock}─╮
        var clock = _timeProvider.GetLocalNow().ToString("HH:mm:ss");
        var paused = _refreshController.IsPaused.CurrentValue;
        var intervalText = paused ? "⏸" : IntervalFormat.Format(_refreshController.Interval.CurrentValue);

        var clockStart = w - 1 - clock.Length - 1;
        var intervalStart = clockStart - 1 - intervalText.Length;

        int tabsEnd;
        if (intervalStart > tabsStart + 4)
        {
            ctx.SetForeground(paused ? theme.Warning : theme.TextDim);
            ctx.WriteAt(intervalStart, 0, intervalText);
            ctx.SetForeground(theme.TextDim);
            ctx.WriteAt(clockStart, 0, clock);
            tabsEnd = intervalStart - 1;
        }
        else if (clockStart > tabsStart + 4)
        {
            ctx.SetForeground(theme.TextDim);
            ctx.WriteAt(clockStart, 0, clock);
            tabsEnd = clockStart - 1;
        }
        else
        {
            // No room for clock — tabs fill up to the right corner
            tabsEnd = w - 2;
        }

        // 4. Tabs — rendered into a clipped sub-context so they can never overwrite clock/corners
        var tabsAvailable = tabsEnd - tabsStart;
        if (tabsAvailable >= 4)
        {
            var tabsCtx = ctx.CreateSubContext(new Rect(tabsStart, 0, tabsAvailable, 1));
            RenderTabs(tabsCtx, theme, tabsAvailable);
        }

        ctx.ResetColors();
    }

    private static void RenderTabs(IRenderContext ctx, ThemeDefinition theme, int available)
    {
        var labels = TabRegistry.Labels;
        if (labels.Count == 0)
        {
            return;
        }

        var active = TabRegistry.CurrentTabIndex;
        var window = TabScrollWindow.Compute(
            labels.Select(l => l.Length).ToArray(), active, available);

        if (window.Last < window.First)
        {
            return;
        }

        var x = 0;

        if (window.HasLeft)
        {
            ctx.SetForeground(theme.Accent);
            ctx.WriteAt(x, 0, '◀');
            x++;
            ctx.SetForeground(theme.Border);
            ctx.WriteAt(x, 0, '─');
            x++;
        }

        for (var i = window.First; i <= window.Last; i++)
        {
            // separator between tabs (not before the first one)
            if (i > window.First)
            {
                ctx.SetForeground(theme.Border);
                ctx.WriteAt(x, 0, '─');
                x++;
            }

            var label = labels[i].ToLowerInvariant();
            if (i == active)
            {
                ctx.SetForeground(theme.Border);
                ctx.WriteAt(x, 0, '┤');
                x++;
                ctx.SetForeground(theme.SelectionText);
                ctx.SetBackground(theme.Selection);
                ctx.WriteAt(x, 0, $" {label} ");
                ctx.ResetColors();
                x += label.Length + 2;
                ctx.SetForeground(theme.Border);
                ctx.WriteAt(x, 0, '├');
                x++;
            }
            else
            {
                ctx.SetForeground(theme.TextDim);
                ctx.WriteAt(x, 0, $" {label} ");
                x += label.Length + 2;
            }
        }

        if (window.HasRight)
        {
            ctx.SetForeground(theme.Border);
            ctx.WriteAt(x, 0, '─');
            x++;
            ctx.SetForeground(theme.Accent);
            ctx.WriteAt(x, 0, '▶');
        }
    }
}
