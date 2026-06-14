using Puhu.Setup;

namespace Puhu.Tests;

public sealed class StartRouteDeciderTests
{
    [Fact]
    public void SetupIncomplete_ReturnsSetupRoute()
    {
        Assert.Equal("/setup", StartRouteDecider.Decide(setupComplete: false, firstTabRoute: "/system"));
    }

    [Fact]
    public void SetupComplete_ReturnsFirstTab()
    {
        Assert.Equal("/system", StartRouteDecider.Decide(setupComplete: true, firstTabRoute: "/system"));
    }
}
