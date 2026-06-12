using Akka.Actor;

namespace Puhu.Plugin;

/// <summary>Marker type for resolving the TickRouter from IActorRegistry.</summary>
public sealed class TickRouterKey;

/// <summary>Register an actor to receive periodic tick messages.</summary>
public sealed record RegisterMonitor(string Key, IActorRef Actor, bool AlwaysOn, TimeSpan? MinInterval);
