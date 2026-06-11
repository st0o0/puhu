using Akka.Actor;
using Servus.Plugin.Sdk;

namespace Servus.Runner;

public sealed class PluginActorContextImpl(
    ActorSystem system,
    IServiceProvider services,
    ITickSource tickSource) : Servus.Plugin.Sdk.IActorContext
{
    public IServiceProvider ServiceProvider => services;
    public ITickSource TickSource => tickSource;

    public void RegisterActor(string name, Func<object> propsFactory)
    {
        var props = (Props)propsFactory();
        system.ActorOf(props, name);
    }
}
