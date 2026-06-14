using Puhu.Nodes;
using Puhu.Plugin;

namespace Puhu.Tests.Tabs;

public sealed class TabOrderTests
{
    private static readonly PluginTabInfo[] Tabs =
    [
        new("Marketplace", "/marketplace"),
        new("Settings", "/settings"),
        new("System", "/system"),
    ];

    [Fact]
    public void EmptySavedOrder_KeepsOriginalOrder()
    {
        var result = TabOrder.Apply([], Tabs);

        Assert.Equal(["/marketplace", "/settings", "/system"], result.Select(t => t.Route));
    }

    [Fact]
    public void SavedOrder_ReordersKnownTabs()
    {
        var result = TabOrder.Apply(["/system", "/settings", "/marketplace"], Tabs);

        Assert.Equal(["/system", "/settings", "/marketplace"], result.Select(t => t.Route));
    }

    [Fact]
    public void NewTabNotInSavedOrder_IsAppendedAtEnd()
    {
        var result = TabOrder.Apply(["/system", "/settings"], Tabs);

        Assert.Equal(["/system", "/settings", "/marketplace"], result.Select(t => t.Route));
    }

    [Fact]
    public void SavedRouteThatNoLongerExists_IsDropped()
    {
        var result = TabOrder.Apply(["/gone", "/system", "/settings", "/marketplace"], Tabs);

        Assert.Equal(["/system", "/settings", "/marketplace"], result.Select(t => t.Route));
    }

    [Fact]
    public void DuplicateSavedRoutes_AppearOnce()
    {
        var result = TabOrder.Apply(["/system", "/system", "/settings", "/marketplace"], Tabs);

        Assert.Equal(["/system", "/settings", "/marketplace"], result.Select(t => t.Route));
    }
}
