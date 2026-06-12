using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;

namespace Puhu.Tests;

public sealed class PluginLoaderTests
{
    [Fact]
    public void DiscoverAndConfigure_FindsBuiltInPlugins()
    {
        var services = new ServiceCollection();
        var builtIns = new List<IPuhuPlugin> { new FakePlugin("Test") };

        var registry = PluginLoader.DiscoverAndConfigure(services, builtIns);

        Assert.Single(registry.LoadedPlugins);
        Assert.Single(registry.PluginTabs);
        Assert.Equal("/test", registry.PluginTabs[0].Route);
    }

    [Fact]
    public void DiscoverAndConfigure_SkipsFailingPlugins()
    {
        var services = new ServiceCollection();
        var builtIns = new List<IPuhuPlugin> { new ThrowingPlugin(), new FakePlugin("Good") };

        var registry = PluginLoader.DiscoverAndConfigure(services, builtIns);

        Assert.Single(registry.LoadedPlugins);
        Assert.Equal("/good", registry.PluginTabs[0].Route);
    }

    [Fact]
    public void DiscoverAndConfigure_EmptyBuiltIns_ReturnsEmptyRegistry()
    {
        var services = new ServiceCollection();

        var registry = PluginLoader.DiscoverAndConfigure(services, []);

        Assert.Empty(registry.LoadedPlugins);
    }

    private sealed class FakePlugin(string name) : IPuhuPlugin
    {
        public string Name => name;
        public void Configure(IPuhuPluginBuilder builder)
        {
            builder.WithTab($"0:{name}", $"/{name.ToLowerInvariant()}");
        }
    }

    private sealed class ThrowingPlugin : IPuhuPlugin
    {
        public string Name => "Broken";
        public void Configure(IPuhuPluginBuilder builder) => throw new Exception("boom");
    }

    private sealed class SettingsAwarePlugin : IPuhuPlugin
    {
        public string Name => "My Plugin";
        public void Configure(IPuhuPluginBuilder builder)
        {
            builder.WithSettings("My Settings", "/settings/my-plugin");
        }
    }

    [Fact]
    public void DiscoverAndConfigure_CollectsSettings()
    {
        var services = new ServiceCollection();
        var builtIns = new List<IPuhuPlugin> { new SettingsAwarePlugin() };

        var registry = PluginLoader.DiscoverAndConfigure(services, builtIns);

        Assert.Single(registry.PluginSettings);
        Assert.Equal("My Settings", registry.PluginSettings[0].Label);
        Assert.Equal("/settings/my-plugin", registry.PluginSettings[0].Route);
        Assert.Equal("my-plugin", registry.PluginSettings[0].PluginName);
    }
}
