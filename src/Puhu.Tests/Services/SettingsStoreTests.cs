using Puhu.Services;
using R3;

namespace Puhu.Tests.Services;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly string _filePath;

    public SettingsStoreTests()
    {
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "settings.json");
    }

    public void Dispose() => Directory.Delete(_tempDir, true);

    [Fact]
    public void Get_ReturnsNull_WhenKeyNotSet()
    {
        var store = new SettingsStore(_filePath);
        Assert.Null(store.Get<string>("missing"));
    }

    [Fact]
    public void Set_Then_Get_ReturnsValue()
    {
        var store = new SettingsStore(_filePath);
        store.Set("foo.bar", 42);
        Assert.Equal(42, store.Get<int>("foo.bar"));
    }

    [Fact]
    public void Set_PersistsToFile()
    {
        var store = new SettingsStore(_filePath);
        store.Set("section.key", "value");

        var store2 = new SettingsStore(_filePath);
        Assert.Equal("value", store2.Get<string>("section.key"));
    }

    [Fact]
    public void Remove_DeletesKey()
    {
        var store = new SettingsStore(_filePath);
        store.Set("section.key", "value");
        store.Remove("section.key");
        Assert.Null(store.Get<string>("section.key"));
    }

    [Fact]
    public void Get_WithNestedKey_ReturnsValue()
    {
        var store = new SettingsStore(_filePath);
        store.Set("marketplace.refresh-interval", 30);
        Assert.Equal(30, store.Get<int>("marketplace.refresh-interval"));
    }

    [Fact]
    public void Observe_EmitsOnSet()
    {
        var store = new SettingsStore(_filePath);
        var received = new List<int>();

        store.Observe<int>("section.key").Subscribe(v => received.Add(v));
        store.Set("section.key", 10);
        store.Set("section.key", 20);

        Assert.Equal([10, 20], received);
    }

    [Fact]
    public void Observe_EmitsCurrentValue_IfAlreadySet()
    {
        var store = new SettingsStore(_filePath);
        store.Set("section.key", 42);

        var received = new List<int>();
        store.Observe<int>("section.key").Subscribe(v => received.Add(v));

        Assert.Equal([42], received);
    }
}
