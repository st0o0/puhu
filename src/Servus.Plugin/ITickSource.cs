using R3;

namespace Servus.Plugin;

public interface ITickSource
{
    TimeSpan CurrentInterval { get; }
    Observable<Tick> Ticks { get; }
    IDisposable Subscribe(Action onTick);
}
