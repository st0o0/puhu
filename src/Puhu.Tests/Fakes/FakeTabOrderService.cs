using Puhu.Plugin;
using R3;

namespace Puhu.Tests.Fakes;

/// <summary>In-memory ITabOrderService for tests. Move uses remove+insert with clamp.</summary>
public sealed class FakeTabOrderService : ITabOrderService
{
    private readonly List<TabDescriptor> _tabs;
    private readonly Subject<Unit> _changed = new();

    public FakeTabOrderService(params (string Label, string Route)[] tabs)
    {
        _tabs = tabs.Select(t => new TabDescriptor(t.Label, t.Route)).ToList();
    }

    public IReadOnlyList<TabDescriptor> Tabs => _tabs;
    public Observable<Unit> Changed => _changed;
    public int MoveCount { get; private set; }

    public void Move(int index, int delta)
    {
        if (index < 0 || index >= _tabs.Count) return;
        var target = Math.Clamp(index + delta, 0, _tabs.Count - 1);
        if (target == index) return;

        var item = _tabs[index];
        _tabs.RemoveAt(index);
        _tabs.Insert(target, item);
        MoveCount++;
        _changed.OnNext(Unit.Default);
    }
}
