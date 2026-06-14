using Puhu.Plugin;
using Puhu.Settings;
using R3;

namespace Puhu.Tests;

public sealed class SetupWizardStateTests
{
    [Fact]
    public void IsComplete_FalseWhenUnset()
    {
        Assert.False(SetupWizardState.IsComplete(new FakeStore()));
    }

    [Fact]
    public void IsComplete_TrueWhenVersionAtOrAboveCurrent()
    {
        var store = new FakeStore();
        store.Set(SetupWizardState.VersionKey, SetupWizardState.CurrentVersion);

        Assert.True(SetupWizardState.IsComplete(store));
    }

    [Fact]
    public void MarkComplete_WritesCurrentVersion()
    {
        var store = new FakeStore();

        SetupWizardState.MarkComplete(store);

        Assert.Equal(SetupWizardState.CurrentVersion, store.Get<int?>(SetupWizardState.VersionKey));
    }

    private sealed class FakeStore : ISettingsStore
    {
        private readonly Dictionary<string, object?> _data = new();
        public T? Get<T>(string key) => _data.TryGetValue(key, out var v) ? (T?)v : default;
        public void Set<T>(string key, T value) => _data[key] = value;
        public Observable<T> Observe<T>(string key) => Observable.Empty<T>();
        public void Remove(string key) => _data.Remove(key);
    }
}
