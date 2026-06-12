using Termina.Terminal;

namespace Puhu.Plugin;

public interface IThemeService
{
    ThemeDefinition Current { get; }
}

public sealed record ThemeDefinition
{
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
}