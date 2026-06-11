using Microsoft.Extensions.DependencyInjection;
using Servus.Plugin.Sdk;

namespace Servus.Runner.Tests;

public sealed class PluginRegistryTests
{
    private static ITickSource StubTickSource() => new StubTick();

    [Fact]
    public void Empty_Registry_HasNoTabs()
    {
        var registry = new PluginRegistry([]);
        Assert.Empty(registry.PluginTabs);
    }

    [Fact]
    public void Registry_CollectsTabsFromBuilders()
    {
        var services = new ServiceCollection();
        var b1 = new ServusPluginBuilder(services, StubTickSource());
        b1.WithTab("Overview", "/overview", ConsoleKey.D0);
        var b2 = new ServusPluginBuilder(services, StubTickSource());
        b2.WithTab("Processes", "/process", ConsoleKey.D1);

        var registry = new PluginRegistry([b1, b2]);

        Assert.Equal(2, registry.PluginTabs.Count);
        Assert.Equal("/overview", registry.PluginTabs[0].Route);
        Assert.Equal("/process", registry.PluginTabs[1].Route);
    }

    [Fact]
    public void Builder_WithoutTab_IsNotInPluginTabs()
    {
        var services = new ServiceCollection();
        var builder = new ServusPluginBuilder(services, StubTickSource());
        builder.WithServices(s => s.AddSingleton("hello"));

        var registry = new PluginRegistry([builder]);
        Assert.Empty(registry.PluginTabs);
    }

    [Fact]
    public void Builder_StoresRouteSetup()
    {
        var services = new ServiceCollection();
        var builder = new ServusPluginBuilder(services, StubTickSource());
        var invoked = false;
        builder.ConfigureRoutes(_ => invoked = true);
        builder.RouteSetup?.Invoke(null!);
        Assert.True(invoked);
    }

    [Fact]
    public void Builder_StoresActorSetup()
    {
        var services = new ServiceCollection();
        var builder = new ServusPluginBuilder(services, StubTickSource());
        var invoked = false;
        builder.ConfigureActors(_ => invoked = true);
        builder.ActorSetup?.Invoke(null!);
        Assert.True(invoked);
    }

    [Fact]
    public void Builder_StoresServiceSetup()
    {
        var services = new ServiceCollection();
        var builder = new ServusPluginBuilder(services, StubTickSource());
        var invoked = false;
        builder.WithServices(_ => invoked = true);
        builder.ServiceSetup?.Invoke(null!);
        Assert.True(invoked);
    }

    private sealed class StubTick : ITickSource
    {
        public TimeSpan CurrentInterval => TimeSpan.FromSeconds(1);
        public IDisposable Subscribe(Action onTick) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }
}
