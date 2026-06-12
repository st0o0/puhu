using Puhu.Plugin;
using R3;
using Termina.Layout;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public sealed class SettingsPage : ReactivePage<SettingsViewModel>, IKeyHintProvider
{
    private readonly ITabNavigator _tabNavigator;
    private readonly IThemeService _themeService;
    private readonly IRefreshController _refreshController;

    public SettingsPage(ITabNavigator tabNavigator, IThemeService themeService, IRefreshController refreshController)
    {
        _tabNavigator = tabNavigator;
        _themeService = themeService;
        _refreshController = refreshController;
    }

    public string[] GetKeyHints() => ["↑↓:Theme", "Enter:Save", "Esc:Quit", "Tab:Switch"];

    public override ILayoutNode BuildLayout()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("themes").WithForeground(theme.TextDim).Bold().Height(1),
        };

        for (var i = 0; i < ViewModel.Themes.Count; i++)
        {
            var name = ViewModel.Themes[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var isSaved = string.Equals(name, ViewModel.SavedTheme.Value, StringComparison.OrdinalIgnoreCase);
            var marker = isSelected ? "▸" : " ";
            var suffix = isSaved ? "  ●" : "";

            rows.Add(new TextNode($"{marker} {name}{suffix}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        if (ViewModel.Themes.Count == 0)
        {
            rows.Add(new TextNode("no themes found").WithForeground(theme.TextDim).Height(1));
        }

        return Layouts.Vertical(rows.ToArray());
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        KeyBindings.RegisterGlobalKeys(
            () => ViewModel.RequestShutdown(),
            path => Navigate(path),
            _tabNavigator,
            _refreshController);

        KeyBindings.Register(ConsoleKey.UpArrow, () => ViewModel.MoveSelection(-1));
        KeyBindings.Register(ConsoleKey.DownArrow, () => ViewModel.MoveSelection(1));
        KeyBindings.Register(ConsoleKey.Enter, () => ViewModel.SaveSelected());

        ViewModel.SelectedIndex.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.SavedTheme.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
    }
}
