using System.Reflection;
using Puhu.Plugin;
using R3;

namespace Puhu.Themes;

public sealed class ThemeService : IThemeService
{
    private const string ThemeSettingKey = "puhu.theme";

    // name -> a provider that yields the raw .theme content. Built-ins read an
    // embedded resource; user themes read a file. A later registration of the
    // same name wins, so user folders can override a built-in.
    private readonly Dictionary<string, Func<string>> _themeSources = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISettingsStore? _settings;
    private readonly Subject<ThemeDefinition> _changes = new();

    public ThemeService(ISettingsStore? settings = null)
    {
        _settings = settings;
    }

    public ThemeDefinition Current { get; private set; } = new();

    public string? CurrentThemeName { get; private set; }

    public Observable<ThemeDefinition> Changes => _changes.AsObservable();

    public IReadOnlyCollection<string> AvailableThemes => _themeSources.Keys;

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

    /// <summary>
    /// Registers the themes embedded in this assembly (resource path
    /// <c>*.Themes.&lt;name&gt;.theme</c>). Call before <see cref="LoadFromDirectory"/>
    /// so user folders can override a built-in of the same name.
    /// </summary>
    public void LoadBuiltIns()
    {
        var assembly = typeof(ThemeService).Assembly;

        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.EndsWith(".theme", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // "Puhu.Themes.gruvbox-dark.theme" -> "gruvbox-dark"
            var stem = resource[..^".theme".Length];
            var name = stem[(stem.LastIndexOf('.') + 1)..];
            _themeSources[name] = () => ReadResource(assembly, resource);
        }
    }

    public void LoadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(directory, "*.theme"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            _themeSources[name] = () => File.ReadAllText(file);
        }
    }

    public bool ApplyByName(string name)
    {
        if (!_themeSources.TryGetValue(name, out var source))
        {
            return false;
        }

        Current = BtopThemeParser.Parse(source());
        CurrentThemeName = name;
        _changes.OnNext(Current);
        return true;
    }

    private static string ReadResource(Assembly assembly, string resource)
    {
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded theme '{resource}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
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
