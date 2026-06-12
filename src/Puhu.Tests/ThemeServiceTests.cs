using Puhu.Plugin;
using Puhu.Themes;
using R3;

namespace Puhu.Tests;

public sealed class ThemeServiceTests : IDisposable
{
    private readonly string _themeDir = Path.Combine(Path.GetTempPath(), $"puhu-themes-{Guid.NewGuid():N}");

    public ThemeServiceTests()
    {
        Directory.CreateDirectory(_themeDir);
        File.WriteAllText(Path.Combine(_themeDir, "testtheme.theme"),
            """theme[main_fg]="#aabbcc" """);
    }

    public void Dispose() => Directory.Delete(_themeDir, recursive: true);

    [Fact]
    public void ApplyByName_KnownTheme_EmitsChange()
    {
        var service = new ThemeService();
        service.LoadFromDirectory(_themeDir);
        ThemeDefinition? observed = null;
        using var sub = service.Changes.Subscribe(t => observed = t);

        var applied = service.ApplyByName("testtheme");

        Assert.True(applied);
        Assert.NotNull(observed);
        Assert.Equal("testtheme", service.CurrentThemeName);
    }

    [Fact]
    public void ApplyByName_UnknownTheme_ReturnsFalseAndKeepsCurrent()
    {
        var service = new ThemeService();
        var before = service.Current;

        Assert.False(service.ApplyByName("nope"));
        Assert.Same(before, service.Current);
    }

    [Fact]
    public void SaveCurrent_PersistsThemeName()
    {
        var store = new FakeSettingsStore();
        var service = new ThemeService(store);
        service.LoadFromDirectory(_themeDir);
        service.ApplyByName("testtheme");

        service.SaveCurrent();

        Assert.Equal("testtheme", store.Get<string>("puhu.theme"));
    }

    [Fact]
    public void RestoreSaved_AppliesPersistedTheme()
    {
        var store = new FakeSettingsStore();
        store.Set("puhu.theme", "testtheme");
        var service = new ThemeService(store);
        service.LoadFromDirectory(_themeDir);

        Assert.True(service.RestoreSaved());
        Assert.Equal("testtheme", service.CurrentThemeName);
    }

    [Fact]
    public void RestoreSaved_NothingPersisted_ReturnsFalse()
    {
        var service = new ThemeService(new FakeSettingsStore());
        service.LoadFromDirectory(_themeDir);

        Assert.False(service.RestoreSaved());
    }

    [Fact]
    public void Apply_Definition_ClearsThemeNameAndEmits()
    {
        var service = new ThemeService();
        service.LoadFromDirectory(_themeDir);
        service.ApplyByName("testtheme");
        ThemeDefinition? observed = null;
        using var sub = service.Changes.Subscribe(t => observed = t);

        service.Apply(new ThemeDefinition());

        Assert.Null(service.CurrentThemeName);
        Assert.NotNull(observed);
    }

    [Fact]
    public void ApplyBuiltIn_DoesNotSetRestorableName()
    {
        var service = new ThemeService();

        service.ApplyBuiltIn("dark");

        Assert.Null(service.CurrentThemeName);
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, object?> _values = [];
        public T? Get<T>(string key) => _values.TryGetValue(key, out var v) ? (T?)v : default;
        public void Set<T>(string key, T value) => _values[key] = value;
        public Observable<T> Observe<T>(string key) => Observable.Empty<T>();
        public void Remove(string key) => _values.Remove(key);
    }
}
