using Puhu.Plugin;
using R3;

namespace Puhu.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ISettingsStore"/> for unit tests. <see cref="SetCount"/> tracks how many
/// times <see cref="Set{T}"/> was called so persistence tests can assert no-op writes are skipped.
/// <see cref="Observe{T}"/> returns an empty stream — tests that need change notifications use the
/// real store or the fixture's <c>InMemorySettingsStore</c>.
/// </summary>
internal sealed class FakeSettingsStore : ISettingsStore
{
    private readonly Dictionary<string, object?> _values = [];

    public int SetCount { get; set; }

    public T? Get<T>(string key) => _values.TryGetValue(key, out var v) ? (T?)v : default;

    public void Set<T>(string key, T value)
    {
        _values[key] = value;
        SetCount++;
    }

    public Observable<T> Observe<T>(string key) => Observable.Empty<T>();

    public void Remove(string key) => _values.Remove(key);
}
