using Puhu.Services;
using R3;

namespace Puhu.Tests.Services;

public sealed class ScopedSettingsStoreTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly SettingsStore _inner;
    private readonly ScopedSettingsStore _scoped;

    public ScopedSettingsStoreTests()
    {
        Directory.CreateDirectory(_tempDir);
        _inner = new SettingsStore(Path.Combine(_tempDir, "settings.json"));
        _scoped = new ScopedSettingsStore(_inner, "marketplace");
    }

    public void Dispose() => Directory.Delete(_tempDir, true);

    [Fact]
    public void Set_PrefixesKeyWithPluginName()
    {
        _scoped.Set("refresh-interval", 30);
        Assert.Equal(30, _inner.Get<int>("marketplace.refresh-interval"));
    }

    [Fact]
    public void Get_ReadsFromPrefixedKey()
    {
        _inner.Set("marketplace.mode", "dark");
        Assert.Equal("dark", _scoped.Get<string>("mode"));
    }

    [Fact]
    public void Remove_RemovesPrefixedKey()
    {
        _scoped.Set("key", "value");
        _scoped.Remove("key");
        Assert.Null(_inner.Get<string>("marketplace.key"));
    }

    [Fact]
    public void Observe_ObservesPrefixedKey()
    {
        var received = new List<int>();
        _scoped.Observe<int>("interval").Subscribe(v => received.Add(v));
        _scoped.Set("interval", 5);
        Assert.Equal([5], received);
    }

    [Fact]
    public void TwoScopes_AreIsolated()
    {
        var other = new ScopedSettingsStore(_inner, "other-plugin");
        _scoped.Set("key", "marketplace-value");
        other.Set("key", "other-value");
        Assert.Equal("marketplace-value", _scoped.Get<string>("key"));
        Assert.Equal("other-value", other.Get<string>("key"));
    }
}
