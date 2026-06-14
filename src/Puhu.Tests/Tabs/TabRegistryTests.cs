using Puhu.Nodes;
using Puhu.Plugin;

namespace Puhu.Tests.Tabs;

[Collection("TabRegistry")]
public sealed class TabRegistryTests
{
    [Fact]
    public void RegisterTabs_ExposesLabelsAndRoutes()
    {
        TabRegistry.RegisterTabs([
            new PluginTabInfo("Alpha", "/alpha"),
            new PluginTabInfo("Beta", "/beta"),
        ]);

        Assert.Equal(2, TabRegistry.TabCount);
        Assert.Equal(["Alpha", "Beta"], TabRegistry.Labels);
        Assert.Equal("/beta", TabRegistry.GetRoute(1));
    }

    [Fact]
    public void GetRoute_ClampsIndex()
    {
        TabRegistry.RegisterTabs([new PluginTabInfo("Alpha", "/alpha")]);

        Assert.Equal("/alpha", TabRegistry.GetRoute(99));
    }

    [Fact]
    public void RegisterTabs_ResetsCurrentIndex()
    {
        TabRegistry.RegisterTabs([new PluginTabInfo("Alpha", "/alpha"), new PluginTabInfo("Beta", "/beta")]);
        TabRegistry.CurrentTabIndex = 1;

        TabRegistry.RegisterTabs([new PluginTabInfo("Alpha", "/alpha")]);

        Assert.Equal(0, TabRegistry.CurrentTabIndex);
    }

    [Fact]
    public void Reorder_KeepsActiveTabByRoute()
    {
        TabRegistry.RegisterTabs([
            new PluginTabInfo("Alpha", "/alpha"),
            new PluginTabInfo("Beta", "/beta"),
            new PluginTabInfo("Gamma", "/gamma"),
        ]);
        TabRegistry.CurrentTabIndex = 2; // active = /gamma

        TabRegistry.Reorder([
            new PluginTabInfo("Gamma", "/gamma"),
            new PluginTabInfo("Alpha", "/alpha"),
            new PluginTabInfo("Beta", "/beta"),
        ]);

        Assert.Equal(["Gamma", "Alpha", "Beta"], TabRegistry.Labels);
        Assert.Equal(0, TabRegistry.CurrentTabIndex); // still on /gamma
    }

    [Fact]
    public void Reorder_WhenActiveRouteRemoved_ClampsIndex()
    {
        TabRegistry.RegisterTabs([
            new PluginTabInfo("Alpha", "/alpha"),
            new PluginTabInfo("Beta", "/beta"),
        ]);
        TabRegistry.CurrentTabIndex = 1; // active = /beta

        TabRegistry.Reorder([new PluginTabInfo("Alpha", "/alpha")]);

        Assert.Equal(0, TabRegistry.CurrentTabIndex);
    }
}
