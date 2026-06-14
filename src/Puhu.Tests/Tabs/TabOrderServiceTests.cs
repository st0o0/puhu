using Puhu.Nodes;
using Puhu.Plugin;
using Puhu.Services;
using Puhu.Tests.Fakes;
using R3;

namespace Puhu.Tests.Tabs;

[Collection("TabRegistry")]
public sealed class TabOrderServiceTests
{
    private static readonly PluginTabInfo[] Tabs =
    [
        new("Marketplace", "/marketplace"),
        new("Settings", "/settings"),
        new("System", "/system"),
    ];

    [Fact]
    public void Tabs_ExposesInitialOrder()
    {
        var service = new TabOrderService(new FakeSettingsStore(), Tabs);

        Assert.Equal(["/marketplace", "/settings", "/system"], service.Tabs.Select(t => t.Route));
    }

    [Fact]
    public void Move_ReordersTabs()
    {
        TabRegistry.RegisterTabs(Tabs);
        var service = new TabOrderService(new FakeSettingsStore(), Tabs);

        service.Move(0, 2); // /marketplace to the end

        Assert.Equal(["/settings", "/system", "/marketplace"], service.Tabs.Select(t => t.Route));
    }

    [Fact]
    public void Move_PersistsRouteArray()
    {
        TabRegistry.RegisterTabs(Tabs);
        var store = new FakeSettingsStore();
        var service = new TabOrderService(store, Tabs);

        service.Move(2, -1); // /system up one

        Assert.Equal(
            ["/marketplace", "/system", "/settings"],
            store.Get<string[]>("puhu.tab-order")!);
    }

    [Fact]
    public void Move_EmitsChanged()
    {
        TabRegistry.RegisterTabs(Tabs);
        var service = new TabOrderService(new FakeSettingsStore(), Tabs);
        var fired = 0;
        using var _ = service.Changed.Subscribe(_ => fired++);

        service.Move(0, 1);

        Assert.Equal(1, fired);
    }

    [Fact]
    public void Move_NoOpWhenClampedToSamePosition_DoesNotEmit()
    {
        TabRegistry.RegisterTabs(Tabs);
        var service = new TabOrderService(new FakeSettingsStore(), Tabs);
        var fired = 0;
        using var _ = service.Changed.Subscribe(_ => fired++);

        service.Move(0, -1); // already at top

        Assert.Equal(0, fired);
    }
}
