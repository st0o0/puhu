using Puhu.Plugin;
using R3;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public sealed class SettingsViewModel : ReactiveViewModel
{
    private readonly IThemeService _themeService;

    public IReadOnlyList<string> Themes { get; }
    public ReactiveProperty<int> SelectedIndex { get; }
    public ReactiveProperty<string?> SavedTheme { get; }

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        Themes = themeService.AvailableThemes
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentIndex = 0;
        if (_themeService.CurrentThemeName is { } name)
        {
            var idx = ((List<string>)Themes).FindIndex(t => t.Equals(name, StringComparison.OrdinalIgnoreCase));
            currentIndex = Math.Max(0, idx);
        }

        SelectedIndex = new ReactiveProperty<int>(currentIndex);
        SavedTheme = new ReactiveProperty<string?>(_themeService.CurrentThemeName);
    }

    public void MoveSelection(int delta)
    {
        if (Themes.Count == 0)
        {
            return;
        }

        var next = Math.Clamp(SelectedIndex.Value + delta, 0, Themes.Count - 1);
        SelectedIndex.Value = next;
        _themeService.ApplyByName(Themes[next]);
    }

    public void SaveSelected()
    {
        if (Themes.Count == 0)
        {
            return;
        }

        _themeService.SaveCurrent();
        SavedTheme.Value = Themes[SelectedIndex.Value];
    }

    public override void OnActivated() { }

    public override void Dispose()
    {
        SelectedIndex.Dispose();
        SavedTheme.Dispose();
        base.Dispose();
    }
}
