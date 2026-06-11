using Akka.Actor;
using Servus.Plugin;

namespace Servus.TUI.Actors;

public sealed class TickRouter : ReceiveActor
{
    private readonly Dictionary<string, (IActorRef Actor, bool AlwaysOn, TimeSpan? MinInterval)> _monitors = new();
    private readonly Dictionary<string, int> _demand = new();

    public TickRouter()
    {
        Receive<RegisterMonitor>(m => _monitors[m.Key] = (m.Actor, m.AlwaysOn, m.MinInterval));
        Receive<DemandChanged>(m => _demand[m.Key] = Math.Max(0, _demand.GetValueOrDefault(m.Key) + m.Delta));
        Receive<Tick>(tick =>
        {
            foreach (var (key, reg) in _monitors)
            {
                if (!reg.AlwaysOn && _demand.GetValueOrDefault(key) == 0)
                {
                    continue;
                }

                if (reg.MinInterval is { } min)
                {
                    var every = Math.Max(1,
                        (int)Math.Ceiling(min.TotalMilliseconds / tick.BaseInterval.TotalMilliseconds));
                    if (tick.Seq % every != 0)
                    {
                        continue;
                    }
                }

                reg.Actor.Tell(tick);
            }
        });
    }
}