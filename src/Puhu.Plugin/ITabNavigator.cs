namespace Puhu.Plugin;

public interface ITabNavigator
{
    bool HasTabs { get; }
    void CycleTab(Action<string> navigate, int delta);
}
