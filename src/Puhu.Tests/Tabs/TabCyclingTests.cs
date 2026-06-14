using Puhu.Tests.Fixtures;
using Puhu.Tests.Helpers;

namespace Puhu.Tests.Tabs;

/// <summary>
/// End-to-end coverage for the global Tab / Shift+Tab tab-cycling keys, driven through a real
/// VirtualInputSource. The fixture is given two tabbed pages (Settings ↔ Marketplace), both of
/// which re-register the global keys, so cycling keeps working across navigations.
/// </summary>
public sealed class TabCyclingTests
{
    private static async Task<PuhuAppFixture> StartAsync()
    {
        var app = new PuhuAppFixture().WithTabs("/settings", "/marketplace");
        await app.StartAsync("/settings");
        await app.Terminal.WaitForTextAsync("themes"); // Settings default (Themes) view
        return app;
    }

    [Fact]
    public async Task Tab_NavigatesToNextTab()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Tab);

        await app.Terminal.WaitForTextAsync("Alpha Plugin"); // Marketplace browse view
    }

    [Fact]
    public async Task Tab_FullCycle_ReturnsToStart()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Tab);
        await app.Terminal.WaitForTextAsync("Alpha Plugin");

        app.SendKey(ConsoleKey.Tab);

        await app.Terminal.WaitForTextAsync("themes");
    }

    [Fact]
    public async Task ShiftTab_NavigatesToPreviousTab()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Tab, shift: true);

        await app.Terminal.WaitForTextAsync("Alpha Plugin"); // wraps to the other tab
    }
}
