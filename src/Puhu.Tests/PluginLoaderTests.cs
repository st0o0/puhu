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
}
