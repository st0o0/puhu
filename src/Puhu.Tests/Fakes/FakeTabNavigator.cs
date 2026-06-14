using Puhu.Plugin;

namespace Puhu.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ITabNavigator"/> for tests. Constructed with a route list it cycles through
/// them (tracking <see cref="CurrentIndex"/>); constructed with no routes it behaves as a null
/// navigator (<see cref="HasTabs"/> false, <see cref="CycleTab"/> a no-op).
/// </summary>
internal sealed class FakeTabNavigator(string[]? routes = null, int currentIndex = 0) : ITabNavigator
{
    private readonly string[] _routes = routes ?? [];

    public int CurrentIndex { get; private set; } = currentIndex;
    public bool HasTabs => _routes.Length > 0;

    public void CycleTab(Action<string> navigate, int delta)
    {
        if (_routes.Length == 0)
        {
            return;
        }
        CurrentIndex = (CurrentIndex + delta + _routes.Length) % _routes.Length;
        navigate(_routes[CurrentIndex]);
    }
}
