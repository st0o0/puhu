using Puhu.Tests.Fixtures;
using Puhu.Tests.Helpers;

namespace Puhu.Tests.Pages.Marketplace;

/// <summary>
/// End-to-end coverage for the Marketplace page key bindings, driven through a real
/// VirtualInputSource. The marketplace actor is wired to <c>ActorRefs.Nobody</c>, so the
/// action keys (install/refresh/update/uninstall/cycle) route harmlessly — they are covered
/// for liveness (the app keeps responding) since their effects are owned by the actor.
/// </summary>
public sealed class MarketplacePageKeyTests
{
    private static async Task<PuhuAppFixture> StartAsync()
    {
        var app = new PuhuAppFixture();
        await app.StartAsync("/marketplace");
        return app;
    }

    [Fact]
    public async Task Browse_ShowsSeededPlugins()
    {
        await using var app = await StartAsync();
        await app.Terminal.WaitForTextAsync("Alpha Plugin");
    }

    [Fact]
    public async Task SubNav_2_ShowsInstalledView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D2);
        await app.Terminal.WaitForTextAsync("No plugins installed.");
    }

    [Fact]
    public async Task SubNav_3_ShowsSourcesView()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D3);
        await app.Terminal.WaitForTextAsync("Registries");
    }

    [Fact]
    public async Task SubNav_1_ReturnsToBrowse()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D3);
        await app.Terminal.WaitForTextAsync("Registries");

        app.SendKey(ConsoleKey.D1);

        await app.Terminal.WaitForTextAsync("Alpha Plugin");
    }

    [Fact]
    public async Task Browse_DownArrow_MovesSelection()
    {
        await using var app = await StartAsync();
        await app.Terminal.WaitForTextAsync("▸ Alpha Plugin");

        app.SendKey(ConsoleKey.DownArrow);

        await app.Terminal.WaitForTextAsync("▸ Beta Plugin");
    }

    [Fact]
    public async Task Browse_Enter_ExpandsSelectedPlugin()
    {
        await using var app = await StartAsync();
        await app.Terminal.WaitForTextAsync("▸ Alpha Plugin");

        app.SendKey(ConsoleKey.Enter);

        await app.Terminal.WaitForTextAsync("First test plugin");
    }

    [Fact]
    public async Task Browse_ActionKeys_KeepAppResponsive()
    {
        await using var app = await StartAsync();
        await app.Terminal.WaitForTextAsync("Alpha Plugin");

        app.SendKey(ConsoleKey.R); // refresh
        app.SendKey(ConsoleKey.I); // install selected

        // Liveness: a view switch still renders, proving the loop survived the actions.
        app.SendKey(ConsoleKey.D2);
        await app.Terminal.WaitForTextAsync("No plugins installed.");
    }

    [Fact]
    public async Task Installed_ActionKeys_KeepAppResponsive()
    {
        await using var app = await StartAsync();
        app.SendKey(ConsoleKey.D2);
        await app.Terminal.WaitForTextAsync("No plugins installed.");

        app.SendKey(ConsoleKey.U); // update
        app.SendKey(ConsoleKey.X); // uninstall
        app.SendKey(ConsoleKey.C); // cycle policy

        app.SendKey(ConsoleKey.D1);
        await app.Terminal.WaitForTextAsync("Alpha Plugin");
    }

    [Fact]
    public async Task Escape_QuitsApp()
    {
        await using var app = await StartAsync();
        await app.Terminal.WaitForTextAsync("Alpha Plugin");

        app.SendKey(ConsoleKey.Escape);

        Assert.True(await app.WaitForShutdownAsync(), "Escape should shut the app down");
    }
}
