using R3;

namespace Puhu.Plugin;

public interface ISettingsStore
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
    Observable<T> Observe<T>(string key);
    void Remove(string key);
}
