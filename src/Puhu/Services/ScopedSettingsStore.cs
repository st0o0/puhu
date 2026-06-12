using Puhu.Plugin;
using R3;

namespace Puhu.Services;

public sealed class ScopedSettingsStore(SettingsStore inner, string pluginName) : ISettingsStore
{
    public T? Get<T>(string key) => inner.Get<T>($"{pluginName}.{key}");
    public void Set<T>(string key, T value) => inner.Set($"{pluginName}.{key}", value);
    public Observable<T> Observe<T>(string key) => inner.Observe<T>($"{pluginName}.{key}");
    public void Remove(string key) => inner.Remove($"{pluginName}.{key}");
}
