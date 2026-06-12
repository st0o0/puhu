using R3;
using Termina.Terminal;

namespace Puhu.Plugin;

public interface IThemeService
{
    ThemeDefinition Current { get; }

    /// <summary>Name des aktiven Themes (null bei direktem Apply einer Definition).</summary>
    string? CurrentThemeName { get; }

    /// <summary>Feuert bei jedem Theme-Wechsel — UI nutzt das für Live-Rerender.</summary>
    Observable<ThemeDefinition> Changes { get; }

    /// <summary>Namen aller geladenen .theme-Dateien.</summary>
    IReadOnlyCollection<string> AvailableThemes { get; }

    /// <summary>Theme live anwenden (ohne Persistenz). False wenn unbekannt.</summary>
    bool ApplyByName(string name);

    /// <summary>Aktives Theme als Nutzer-Auswahl persistieren.</summary>
    void SaveCurrent();
}

public sealed record ThemeDefinition
{
    private static readonly Gradient DefaultGraphGradient =
        Gradient.Create(Color.FromHex("#50fa7b"), Color.FromHex("#f1fa8c"), Color.FromHex("#ff5555"));

    public Color Background { get; init; } = Color.Default;
    public Color Foreground { get; init; } = Color.White;
    public Color TextDim { get; init; } = Color.Gray;
    public Color Border { get; init; } = Color.BrightCyan;
    public Color PanelTitle { get; init; } = Color.BrightCyan;
    public Color Selection { get; init; } = Color.BrightCyan;
    public Color SelectionText { get; init; } = Color.Black;
    public Color StatusBar { get; init; } = Color.BrightCyan;
    public Color StatusBarText { get; init; } = Color.Black;
    public Color Warning { get; init; } = Color.BrightYellow;
    public Color Error { get; init; } = Color.BrightRed;
    public Color Success { get; init; } = Color.BrightGreen;
    public Color Header { get; init; } = Color.BrightBlack;
    public Color Accent { get; init; } = Color.Cyan;
    public Gradient GraphGradient { get; init; } = DefaultGraphGradient;
}