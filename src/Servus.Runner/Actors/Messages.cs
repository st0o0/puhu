using Akka.Actor;

namespace Servus.Runner.Actors;

public sealed record DemandChanged(string MonitorKey, int Delta);
public sealed record RegisterMonitor(string Key, IActorRef Actor, bool AlwaysOn, TimeSpan? MinInterval);
