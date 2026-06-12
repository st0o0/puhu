using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;
using Termina.Hosting;

namespace Puhu;

public sealed class PuhuPluginBuilder(IServiceCollection services, string pluginName) : IPuhuPluginBuilder
{
    public string PluginName => pluginName;
    public PluginTabInfo? Tab { get; private set; }
    public PluginSettingsInfo? Settings { get; private set; }
    public Action<IServiceCollection>? ServiceSetup { get; private set; }
    public Action<ActorSystem, IActorRegistry, IDependencyResolver>? ActorSetup { get; private set; }
    public Action<TerminaBuilder>? RouteSetup { get; private set; }

    public IPuhuPluginBuilder WithTab(string label, string route)
    {
        Tab = new PluginTabInfo(label, route);
        return this;
    }

    public IPuhuPluginBuilder WithSettings(string label, string route)
    {
        Settings = new PluginSettingsInfo(label, route, pluginName);
        return this;
    }

    public IPuhuPluginBuilder WithServices(Action<IServiceCollection> configure)
    {
        ServiceSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder WithActors(Action<ActorSystem, IActorRegistry, IDependencyResolver> configure)
    {
        ActorSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder WithRoutes(Action<TerminaBuilder> configure)
    {
        RouteSetup = configure;
        return this;
    }
}
