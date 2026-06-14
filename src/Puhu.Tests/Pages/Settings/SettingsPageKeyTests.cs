using Puhu.Tests.Fixtures;
using Puhu.Tests.Helpers;

namespace Puhu.Tests.Pages.Settings;

/// <summary>
/// End-to-end coverage for every key binding on the Settings page, driven through a real
/// <see cref="Termina.Input.VirtualInputSource"/> and asserted on the rendered
/// <see cref="Termina.Terminal.VirtualTerminal"/> (and ViewModel-driven service state).
/// </summary>
public sealed class SettingsPageKeyTests
{
    private static async Task<PuhuAppFixture> StartAsync()
    {
        var app = new PuhuAppFixture();
        await app.StartAsync("/settings");
        return app;
    }

    [Fact]
    public async Task SubNav_1_ShowsThemesView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D1);
        await app.Terminal.WaitForTextAsync("themes");
    }

    [Fact]
    public async Task SubNav_2_ShowsRefreshView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D2);
        await app.Terminal.WaitForTextAsync("refresh rate");
    }

    [Fact]
    public async Task SubNav_3_ShowsTabsView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D3);
        await app.Terminal.WaitForTextAsync("tab order");
    }

    [Fact]
    public async Task SubNav_4_ShowsSetupView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D4);
        await app.Terminal.WaitForTextAsync("re-run setup wizard");
    }

    [Fact]
    public async Task Themes_DownArrow_MovesSelection()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D1);
        await app.Terminal.WaitForTextAsync("▸ btop-default");

        app.SendKey(ConsoleKey.DownArrow);

        await app.Terminal.WaitForTextAsync("▸ catppuccin-mocha");
    }

    [Fact]
    public async Task Themes_UpArrow_MovesSelectionBack()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D1);
        app.SendKey(ConsoleKey.DownArrow);
        await app.Terminal.WaitForTextAsync("▸ catppuccin-mocha");

        app.SendKey(ConsoleKey.UpArrow);

        await app.Terminal.WaitForTextAsync("▸ btop-default");
    }

    [Fact]
    public async Task Themes_Enter_AppliesSelectedTheme()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D1);
        app.SendKey(ConsoleKey.DownArrow);
        await app.Terminal.WaitForTextAsync("▸ catppuccin-mocha");

        app.SendKey(ConsoleKey.Enter);

        await ScreenAssert.WaitUntilAsync(
            () => app.Theme.CurrentThemeName == "catppuccin-mocha",
            "Enter should apply the selected theme");
    }

    [Fact]
    public async Task Refresh_P_TogglesPause()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D2);
        await app.Terminal.WaitForTextAsync("paused: no");

        app.SendKey(ConsoleKey.P);

        await app.Terminal.WaitForTextAsync("paused: yes");
    }

    [Fact]
    public async Task Plus_SpeedsUpRefreshInterval()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Add);

        await ScreenAssert.WaitUntilAsync(
            () => app.Refresh.CurrentInterval == TimeSpan.FromMilliseconds(500),
            "'+' should speed the refresh interval up to 500ms");
    }

    [Fact]
    public async Task Minus_SlowsDownRefreshInterval()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Subtract);

        await ScreenAssert.WaitUntilAsync(
            () => app.Refresh.CurrentInterval == TimeSpan.FromMilliseconds(2000),
            "'-' should slow the refresh interval down to 2s");
    }

    [Fact]
    public async Task Tabs_DownArrow_MovesSelection()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D3);
        await app.Terminal.WaitForTextAsync("▸ system");

        app.SendKey(ConsoleKey.DownArrow);

        await app.Terminal.WaitForTextAsync("▸ marketplace");
    }

    [Fact]
    public async Task Tabs_ShiftDownArrow_ReordersTabs()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D3);
        await app.Terminal.WaitForTextAsync("tab order");

        app.SendKey(ConsoleKey.DownArrow, shift: true);

        await ScreenAssert.WaitUntilAsync(
            () => app.TabOrder.MoveCount == 1,
            "Shift+Down should reorder the selected tab");
    }

    [Fact]
    public async Task Setup_Enter_NavigatesToWizard()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D4);
        await app.Terminal.WaitForTextAsync("re-run setup wizard");

        app.SendKey(ConsoleKey.Enter);

        await app.Terminal.WaitForTextAsync("[welcome]");
    }

    [Fact]
    public async Task Escape_QuitsApp()
    {
        await using var app = await StartAsync();

        app.SendKey(ConsoleKey.Escape);

        Assert.True(await app.WaitForShutdownAsync(), "Escape should shut the app down");
    }
}
