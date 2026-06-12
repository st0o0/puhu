using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;

namespace Puhu.Tests;

public sealed class PluginRegistryTests
{
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
        var b1 = new PuhuPluginBuilder(services, "test");
        b1.WithTab("Overview", "/overview");
        var b2 = new PuhuPluginBuilder(services, "test");
        b2.WithTab("Processes", "/process");

        var registry = new PluginRegistry([b1, b2]);

        Assert.Equal(2, registry.PluginTabs.Count);
        Assert.Equal("/overview", registry.PluginTabs[0].Route);
        Assert.Equal("/process", registry.PluginTabs[1].Route);
    }

    [Fact]
    public void Builder_WithoutTab_IsNotInPluginTabs()
    {
        var services = new ServiceCollection();
        var builder = new PuhuPluginBuilder(services, "test");
        builder.WithServices(s => s.AddSingleton("hello"));

        var registry = new PluginRegistry([builder]);
        Assert.Empty(registry.PluginTabs);
    }

    [Fact]
    public void Builder_StoresRouteSetup()
    {
        var services = new ServiceCollection();
        var builder = new PuhuPluginBuilder(services, "test");
        var invoked = false;
        builder.WithRoutes(_ => invoked = true);
        builder.RouteSetup?.Invoke(null!);
        Assert.True(invoked);
    }

    [Fact]
    public void Builder_StoresActorSetup()
    {
        var services = new ServiceCollection();
        var builder = new PuhuPluginBuilder(services, "test");
        var invoked = false;
        builder.WithActors((_, _, _) => invoked = true);
        builder.ActorSetup?.Invoke(null!, null!, null!);
        Assert.True(invoked);
    }

    [Fact]
    public void Builder_StoresServiceSetup()
    {
        var services = new ServiceCollection();
        var builder = new PuhuPluginBuilder(services, "test");
        var invoked = false;
        builder.WithServices(_ => invoked = true);
        builder.ServiceSetup?.Invoke(null!);
        Assert.True(invoked);
    }

    [Fact]
    public void Builder_StoresSettingsInfo()
    {
        var services = new ServiceCollection();
        var builder = new PuhuPluginBuilder(services, "test-plugin");
        builder.WithSettings("My Settings", "/settings/test-plugin");

        Assert.NotNull(builder.Settings);
        Assert.Equal("My Settings", builder.Settings!.Label);
        Assert.Equal("/settings/test-plugin", builder.Settings.Route);
        Assert.Equal("test-plugin", builder.Settings.PluginName);
    }
}
