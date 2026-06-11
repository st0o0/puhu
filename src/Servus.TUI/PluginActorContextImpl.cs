using Akka.Actor;
using Servus.TUI.Plugin;

namespace Servus.TUI;

public sealed class PluginActorContextImpl(
    IServiceProvider services,
    List<ActorRegistrationInfo> registrations) : Plugin.IActorContext
{
    public IServiceProvider ServiceProvider => services;

    public IActorRegistration RegisterActor(string name, Props props)
    {
        var info = new ActorRegistrationInfo(name, props, null, false);
        registrations.Add(info);
        return new ActorRegistrationBuilder(info, registrations);
    }
}

internal sealed class ActorRegistrationBuilder(
    ActorRegistrationInfo info,
    List<ActorRegistrationInfo> registrations) : IActorRegistration
{
    public IActorRegistration WithTicks(TimeSpan? minInterval = null, bool alwaysOn = false)
    {
        var idx = registrations.IndexOf(info);
        registrations[idx] = info with { MinInterval = minInterval, AlwaysOn = alwaysOn };
        return this;
    }
}
