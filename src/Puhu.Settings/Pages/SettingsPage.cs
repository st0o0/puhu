using Puhu.Plugin;
using Termina.Layout;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public sealed class SettingsPage : ReactivePage<SettingsViewModel>, IKeyHintProvider
{
    private readonly ITabNavigator _tabNavigator;

    public SettingsPage(ITabNavigator tabNavigator)
    {
        _tabNavigator = tabNavigator;
    }

    public string[] GetKeyHints() => ["Esc:Quit", "Tab:Switch"];

    public override ILayoutNode BuildLayout() =>
        new TextNode("Settings — coming soon");

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        KeyBindings.RegisterGlobalKeys(
            () => ViewModel.RequestShutdown(),
            path => Navigate(path),
            _tabNavigator);
    }
}
