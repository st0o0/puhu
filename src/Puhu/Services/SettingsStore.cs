using System.Text.Json;
using Puhu.Plugin;
using R3;

namespace Puhu.Services;

public sealed class SettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, JsonElement> _data = new();
    private readonly Dictionary<string, object> _subjects = new();

    public SettingsStore(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    public T? Get<T>(string key)
    {
        if (!_data.TryGetValue(key, out var element))
        {
            return default;
        }

        return element.Deserialize<T>(JsonOptions);
    }

    public void Set<T>(string key, T value)
    {
        var element = JsonSerializer.SerializeToElement(value, JsonOptions);
        _data[key] = element;
        Save();
        NotifyObservers(key, element);
    }

    public Observable<T> Observe<T>(string key)
    {
        var subject = GetOrCreateSubject<T>(key);
        if (_data.TryGetValue(key, out var element))
        {
            var initial = element.Deserialize<T>(JsonOptions);
            return subject.Prepend(initial!);
        }
        return subject;
    }

    public void Remove(string key)
    {
        _data.Remove(key);
        Save();
    }

    private Subject<T> GetOrCreateSubject<T>(string key)
    {
        if (_subjects.TryGetValue(key, out var existing))
        {
            return (Subject<T>)existing;
        }

        var subject = new Subject<T>();
        _subjects[key] = subject;
        return subject;
    }

    private void NotifyObservers(string key, JsonElement element)
    {
        if (!_subjects.TryGetValue(key, out var subject))
        {
            return;
        }

        var subjectType = subject.GetType();
        var genericArg = subjectType.GetGenericArguments()[0];
        var value = element.Deserialize(genericArg, JsonOptions);
        var onNext = subjectType.GetMethod("OnNext");
        onNext?.Invoke(subject, [value]);
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        var json = File.ReadAllText(_filePath);
        var doc = JsonDocument.Parse(json);
        foreach (var section in doc.RootElement.EnumerateObject())
        {
            if (section.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in section.Value.EnumerateObject())
                {
                    _data[$"{section.Name}.{prop.Name}"] = prop.Value.Clone();
                }
            }
        }
    }

    private void Save()
    {
        _lock.Wait();
        try
        {
            var grouped = new Dictionary<string, Dictionary<string, JsonElement>>();
            foreach (var (key, value) in _data)
            {
                var dotIndex = key.IndexOf('.');
                if (dotIndex < 0)
                {
                    continue;
                }

                var section = key[..dotIndex];
                var prop = key[(dotIndex + 1)..];
                if (!grouped.TryGetValue(section, out var dict))
                {
                    dict = new Dictionary<string, JsonElement>();
                    grouped[section] = dict;
                }
                dict[prop] = value;
            }

            var dir = Path.GetDirectoryName(_filePath);
            if (dir is not null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var tempPath = _filePath + ".tmp";
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(grouped, JsonOptions);
            File.WriteAllBytes(tempPath, jsonBytes);
            File.Move(tempPath, _filePath, overwrite: true);
        }
        finally
        {
            _lock.Release();
        }
    }
}
