using Puhu.Nodes;
using Puhu.Plugin;
using R3;

namespace Puhu.Services;

/// <summary>
/// Owns the live tab order: persists it to <c>puhu.tab-order</c>, keeps the
/// static <see cref="TabRegistry"/> in sync (preserving the active tab), and
/// publishes <see cref="Changed"/> for the UI.
/// </summary>
public sealed class TabOrderService : ITabOrderService, IDisposable
{
    public const string OrderKey = "puhu.tab-order";

    private readonly ISettingsStore _settings;
    private readonly Subject<Unit> _changed = new();
    private readonly List<PluginTabInfo> _tabs;

    public TabOrderService(ISettingsStore settings, IReadOnlyList<PluginTabInfo> orderedTabs)
    {
        _settings = settings;
        _tabs = orderedTabs.ToList();
    }

    public IReadOnlyList<TabDescriptor> Tabs =>
        _tabs.Select(t => new TabDescriptor(t.Label, t.Route)).ToList();

    public Observable<Unit> Changed => _changed;

    public void Move(int index, int delta)
    {
        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        var target = Math.Clamp(index + delta, 0, _tabs.Count - 1);
        if (target == index)
        {
            return;
        }

        var item = _tabs[index];
        _tabs.RemoveAt(index);
        _tabs.Insert(target, item);

        _settings.Set(OrderKey, _tabs.Select(t => t.Route).ToArray());
        TabRegistry.Reorder(_tabs);
        _changed.OnNext(Unit.Default);
    }

    public void Dispose()
    {
        _changed.OnCompleted();
        _changed.Dispose();
    }
}
