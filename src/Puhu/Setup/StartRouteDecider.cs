namespace Puhu.Setup;

/// <summary>Chooses the initial route: the setup wizard on first run, else the first tab.</summary>
public static class StartRouteDecider
{
    public const string SetupRoute = "/setup";

    public static string Decide(bool setupComplete, string firstTabRoute) =>
        setupComplete ? firstTabRoute : SetupRoute;
}
