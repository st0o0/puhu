using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Termina.Hosting;

namespace Puhu.Plugin;

public interface IPuhuPluginBuilder
{
    IPuhuPluginBuilder WithTab(string label, string route);
    IPuhuPluginBuilder WithServices(Action<IServiceCollection> configure);
    IPuhuPluginBuilder WithActors(Action<ActorSystem, IActorRegistry, IDependencyResolver> configure);
    IPuhuPluginBuilder WithRoutes(Action<TerminaBuilder> configure);
}
