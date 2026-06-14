using Puhu.Tests.Fixtures;
using Puhu.Tests.Helpers;

namespace Puhu.Tests.Pages.Splash;

/// <summary>
/// End-to-end coverage for the splash screen's only key binding (Escape → quit), driven
/// through a real VirtualInputSource. The splash progress timer uses a frozen
/// FakeTimeProvider so it stays on screen instead of auto-navigating.
/// </summary>
public sealed class SplashPageKeyTests
{
    [Fact]
    public async Task Renders_QuitHint()
    {
        await using var app = new PuhuAppFixture();
        await app.StartAsync("/splash");

        await app.Terminal.WaitForTextAsync("ESC Quit");
    }

    [Fact]
    public async Task Escape_QuitsApp()
    {
        await using var app = new PuhuAppFixture();
        await app.StartAsync("/splash");
        await app.Terminal.WaitForTextAsync("ESC Quit");

        app.SendKey(ConsoleKey.Escape);

        Assert.True(await app.WaitForShutdownAsync(), "Escape should shut the app down");
    }
}
