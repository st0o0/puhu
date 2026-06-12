using Puhu.Nodes;
using Puhu.Plugin;

namespace Puhu.Tests;

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
}
