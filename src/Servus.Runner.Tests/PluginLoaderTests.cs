using Microsoft.Extensions.DependencyInjection;
using Servus.Plugin.Sdk;

namespace Servus.Runner.Tests;

public sealed class PluginLoaderTests
{
    [Fact]
    public void DiscoverAndConfigure_FindsBuiltInPlugins()
    {
        var services = new ServiceCollection();
        var tickSource = new StubTick();
        var builtIns = new List<IServusPlugin> { new FakePlugin("Test") };

        var registry = PluginLoader.DiscoverAndConfigure(services, tickSource, builtIns);

        Assert.Single(registry.LoadedPlugins);
        Assert.Single(registry.PluginTabs);
        Assert.Equal("/test", registry.PluginTabs[0].Route);
    }

    [Fact]
    public void DiscoverAndConfigure_SkipsFailingPlugins()
    {
        var services = new ServiceCollection();
        var tickSource = new StubTick();
        var builtIns = new List<IServusPlugin> { new ThrowingPlugin(), new FakePlugin("Good") };

        var registry = PluginLoader.DiscoverAndConfigure(services, tickSource, builtIns);

        Assert.Single(registry.LoadedPlugins);
        Assert.Equal("/good", registry.PluginTabs[0].Route);
    }

    [Fact]
    public void DiscoverAndConfigure_EmptyBuiltIns_ReturnsEmptyRegistry()
    {
        var services = new ServiceCollection();
        var tickSource = new StubTick();

        var registry = PluginLoader.DiscoverAndConfigure(services, tickSource, []);

        Assert.Empty(registry.LoadedPlugins);
    }

    private sealed class FakePlugin(string name) : IServusPlugin
    {
        public string Name => name;
        public void Configure(IServusPluginBuilder builder)
        {
            builder.WithTab($"0:{name}", $"/{name.ToLowerInvariant()}");
        }
    }

    private sealed class ThrowingPlugin : IServusPlugin
    {
        public string Name => "Broken";
        public void Configure(IServusPluginBuilder builder) => throw new Exception("boom");
    }

    private sealed class StubTick : ITickSource
    {
        public TimeSpan CurrentInterval => TimeSpan.FromSeconds(1);
        public IDisposable Subscribe(Action onTick) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }
}
