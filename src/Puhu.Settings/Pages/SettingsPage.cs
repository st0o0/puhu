using Termina.Layout;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public sealed class SettingsPage : ReactivePage<SettingsViewModel>
{
    public override ILayoutNode BuildLayout() =>
        new TextNode("Settings — coming soon");
}
