using Puhu.Plugin;
using R3;
using Termina.Terminal;

namespace Puhu.Themes;

public sealed class ThemeService : IThemeService
{
    private const string ThemeSettingKey = "puhu.theme";

    private readonly Dictionary<string, string> _themePaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISettingsStore? _settings;
    private readonly Subject<ThemeDefinition> _changes = new();

    public ThemeService(ISettingsStore? settings = null)
    {
        _settings = settings;
    }

    public ThemeDefinition Current { get; private set; } = new();

    public string? CurrentThemeName { get; private set; }

    public Observable<ThemeDefinition> Changes => _changes;

    public IReadOnlyCollection<string> AvailableThemes => _themePaths.Keys;

    public void Apply(ThemeDefinition theme)
    {
        Current = theme;
        CurrentThemeName = null;
        _changes.OnNext(theme);
    }

    public void ApplyBuiltIn(string theme)
    {
        Current = theme switch
        {
            "light" => new ThemeDefinition
            {
                Background = Color.White,
                Foreground = Color.Black,
                TextDim = Color.DarkGray,
                Border = Color.Blue,
                PanelTitle = Color.Blue,
                Selection = Color.Blue,
                SelectionText = Color.White,
                StatusBar = Color.Blue,
                StatusBarText = Color.White,
                Warning = Color.Yellow,
                Error = Color.Red,
                Success = Color.Green,
                Header = Color.DarkGray,
                Accent = Color.Blue,
            },
            "nord" => new ThemeDefinition
            {
                Background = Color.Default,
                Foreground = Color.White,
                TextDim = Color.BrightBlack,
                Border = Color.Cyan,
                PanelTitle = Color.Cyan,
                Selection = Color.Cyan,
                SelectionText = Color.Black,
                StatusBar = Color.Cyan,
                StatusBarText = Color.Black,
                Warning = Color.BrightYellow,
                Error = Color.BrightRed,
                Success = Color.BrightGreen,
                Header = Color.BrightBlack,
                Accent = Color.BrightCyan,
            },
            _ => new ThemeDefinition(),
        };
        CurrentThemeName = theme;
        _changes.OnNext(Current);
    }

    public void SetTerminalBackground()
    {
        if (Current.Background == Color.Default)
            return;

        var code = Current.Background == Color.White ? "47" : "40";
        Console.Write($"\x1b[{code}m\x1b[2J\x1b[H");
    }

    public static void ResetTerminalBackground()
    {
        Console.Write("\x1b[0m\x1b[2J\x1b[H");
    }

    public void LoadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.GetFiles(directory, "*.theme"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            _themePaths[name] = file;
        }
    }

    public bool ApplyByName(string name)
    {
        if (!_themePaths.TryGetValue(name, out var path))
        {
            return false;
        }

        Current = BtopThemeParser.ParseFile(path);
        CurrentThemeName = name;
        _changes.OnNext(Current);
        return true;
    }

    public void SaveCurrent()
    {
        if (CurrentThemeName is not null)
        {
            _settings?.Set(ThemeSettingKey, CurrentThemeName);
        }
    }

    public bool RestoreSaved()
    {
        var saved = _settings?.Get<string>(ThemeSettingKey);
        return saved is not null && ApplyByName(saved);
    }
}
