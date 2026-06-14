using Puhu.Plugin;
using R3;

namespace Puhu.Tests.Fixtures;

/// <summary>In-memory <see cref="ISettingsStore"/> for app fixtures (no filesystem).</summary>
public sealed class InMemorySettingsStore : ISettingsStore
{
    private readonly Dictionary<string, object?> _values = [];

    public T? Get<T>(string key) => _values.TryGetValue(key, out var v) ? (T?)v : default;
    public void Set<T>(string key, T value) => _values[key] = value;
    public Observable<T> Observe<T>(string key) => Observable.Empty<T>();
    public void Remove(string key) => _values.Remove(key);
}
