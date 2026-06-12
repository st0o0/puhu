using Puhu.Plugin;
using R3;

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

    public Observable<ThemeDefinition> Changes => _changes.AsObservable();

    public IReadOnlyCollection<string> AvailableThemes => _themePaths.Keys;

    public void Apply(ThemeDefinition theme)
    {
        Current = theme;
        CurrentThemeName = null;
        _changes.OnNext(theme);
    }

    public void ApplyBuiltIn(string theme)
    {
        Current = new ThemeDefinition();
        CurrentThemeName = null;
        _changes.OnNext(Current);
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
