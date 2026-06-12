using Puhu.Services;

namespace Puhu.Tests;

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
}
