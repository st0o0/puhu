namespace Puhu.Nodes;

/// <summary>
/// Computes the visible window of the tab bar: the active tab is always visible,
/// neighbors fill the remaining space, arrows signal cut-off tabs.
/// </summary>
internal static class TabScrollWindow
{
    internal readonly record struct Window(int First, int Last, bool HasLeft, bool HasRight);

    public static Window Compute(IReadOnlyList<int> labelLengths, int activeIndex, int available)
    {
        var count = labelLengths.Count;
        if (count == 0)
        {
            return new Window(0, -1, false, false);
        }

        activeIndex = Math.Clamp(activeIndex, 0, count - 1);

        var first = activeIndex;
        var last = activeIndex;
        var cost = CostOf(activeIndex);

        while (true)
        {
            var grown = false;

            if (last < count - 1 && Fits(first, last + 1, cost + 1 + CostOf(last + 1)))
            {
                last++;
                cost += 1 + CostOf(last);
                grown = true;
            }

            if (first > 0 && Fits(first - 1, last, cost + 1 + CostOf(first - 1)))
            {
                first--;
                cost += 1 + CostOf(first);
                grown = true;
            }

            if (!grown)
            {
                break;
            }
        }

        return new Window(first, last, first > 0, last < count - 1);

        int CostOf(int i) => labelLengths[i] + (i == activeIndex ? 4 : 2);

        bool Fits(int f, int l, int c)
        {
            var arrows = (f > 0 ? 2 : 0) + (l < count - 1 ? 2 : 0);
            return c + arrows <= available;
        }
    }
}
