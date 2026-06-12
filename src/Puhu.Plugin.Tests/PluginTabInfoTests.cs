using Puhu.Plugin;

namespace Puhu.Plugin.Tests;

public sealed class PluginTabInfoTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var tab = new PluginTabInfo("Overview", "/overview");

        Assert.Equal("Overview", tab.Label);
        Assert.Equal("/overview", tab.Route);
    }

    [Fact]
    public void Tick_HasSeqAndBaseInterval()
    {
        var tick = new Tick(42, TimeSpan.FromSeconds(1));

        Assert.Equal(42, tick.Seq);
        Assert.Equal(TimeSpan.FromSeconds(1), tick.BaseInterval);
    }

    [Fact]
    public void PluginSettingsInfo_SetsProperties()
    {
        var info = new PluginSettingsInfo("Marketplace", "/settings/marketplace", "marketplace");

        Assert.Equal("Marketplace", info.Label);
        Assert.Equal("/settings/marketplace", info.Route);
        Assert.Equal("marketplace", info.PluginName);
    }
}
