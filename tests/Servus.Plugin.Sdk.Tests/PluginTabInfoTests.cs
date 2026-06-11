namespace Servus.Plugin.Sdk.Tests;

public sealed class PluginTabInfoTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var tab = new PluginTabInfo("Overview", "/overview", ConsoleKey.D0);

        Assert.Equal("Overview", tab.Label);
        Assert.Equal("/overview", tab.Route);
        Assert.Equal(ConsoleKey.D0, tab.HotKey);
    }

    [Fact]
    public void Ctor_HotKeyIsOptional()
    {
        var tab = new PluginTabInfo("Settings", "/settings");

        Assert.Null(tab.HotKey);
    }

    [Fact]
    public void Tick_HasSeqAndBaseInterval()
    {
        var tick = new Tick(42, TimeSpan.FromSeconds(1));

        Assert.Equal(42, tick.Seq);
        Assert.Equal(TimeSpan.FromSeconds(1), tick.BaseInterval);
    }
}
