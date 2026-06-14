using Puhu.Tests.Fixtures;
using Puhu.Tests.Helpers;

namespace Puhu.Tests.Pages.Wizard;

/// <summary>
/// End-to-end coverage for every key binding on the setup wizard. Step navigation
/// (Enter/Tab forward, Escape/Shift+Tab back) is handled by Termina's WizardNode via focus
/// routing; the page adds Up/Down selection, Shift+Up/Down tab reorder, S (skip) and O
/// (toggle marketplace). All driven through a real VirtualInputSource.
/// </summary>
public sealed class SetupWizardPageKeyTests
{
    private static async Task<PuhuAppFixture> StartAsync()
    {
        var app = new PuhuAppFixture();
        await app.StartAsync("/setup");
        await app.Terminal.WaitForTextAsync("[welcome]");
        return app;
    }

    [Fact]
    public async Task Initial_ShowsWelcomeStep()
    {
        await using var app = await StartAsync();
        app.Terminal.Contains("[welcome]");
    }

    [Fact]
    public async Task Enter_AdvancesToNextStep()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Enter);

        await app.Terminal.WaitForTextAsync("[theme]");
    }

    [Fact]
    public async Task Tab_AdvancesToNextStep()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Tab);

        await app.Terminal.WaitForTextAsync("[theme]");
    }

    [Fact]
    public async Task Escape_GoesBackAStep()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.Enter);
        await app.Terminal.WaitForTextAsync("[theme]");

        app.SendKey(ConsoleKey.Escape);

        await app.Terminal.WaitForTextAsync("[welcome]");
    }

    [Fact]
    public async Task ShiftTab_GoesBackAStep()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.Enter);
        await app.Terminal.WaitForTextAsync("[theme]");

        app.SendKey(ConsoleKey.Tab, shift: true);

        await app.Terminal.WaitForTextAsync("[welcome]");
    }

    [Fact]
    public async Task Theme_DownArrow_MovesSelection()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.Enter); // Welcome -> Theme
        await app.Terminal.WaitForTextAsync("▸ btop-default");

        app.SendKey(ConsoleKey.DownArrow);

        await app.Terminal.WaitForTextAsync("▸ catppuccin-mocha");
    }

    [Fact]
    public async Task TabOrder_ShiftDown_ReordersTabs()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.Enter); // Theme
        app.SendKey(ConsoleKey.Enter); // Refresh
        app.SendKey(ConsoleKey.Enter); // TabOrder
        await app.Terminal.WaitForTextAsync("▸ system");

        app.SendKey(ConsoleKey.DownArrow, shift: true);

        await ScreenAssert.WaitUntilAsync(
            () => app.TabOrder.MoveCount == 1,
            "Shift+Down on the tab-order step should reorder a tab");
    }

    [Fact]
    public async Task Plugins_O_TogglesOpenMarketplace()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.Enter); // Theme
        app.SendKey(ConsoleKey.Enter); // Refresh
        app.SendKey(ConsoleKey.Enter); // TabOrder
        app.SendKey(ConsoleKey.Enter); // Plugins
        await app.Terminal.WaitForTextAsync("[ ] Open the Marketplace");

        app.SendKey(ConsoleKey.O);

        await app.Terminal.WaitForTextAsync("[x] Open the Marketplace");
    }

    [Fact]
    public async Task S_SkipsToSystem()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.S);

        await app.Terminal.WaitForTextAsync("system view");
    }
}
