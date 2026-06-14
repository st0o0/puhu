using Puhu.Plugin;
using R3;

namespace Puhu.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IThemeService"/> for tests. Records the last applied name and whether
/// <see cref="SaveCurrent"/> was called so view-model tests can assert on them; node/render
/// tests that only need a theme source construct it with no themes.
/// </summary>
internal sealed class FakeThemeService(IReadOnlyList<string>? themes = null) : IThemeService
{
    public string? Name { get; set; }
    public string? LastApplied { get; private set; }
    public bool Saved { get; private set; }

    public ThemeDefinition Current { get; } = new();
    public string? CurrentThemeName => LastApplied ?? Name;
    public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
    public IReadOnlyCollection<string> AvailableThemes => themes?.ToList() ?? [];

    public bool ApplyByName(string name)
    {
        LastApplied = name;
        return true;
    }

    public void SaveCurrent() => Saved = true;
}
