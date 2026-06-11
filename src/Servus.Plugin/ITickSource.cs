namespace Servus.Plugin.Sdk;

public interface ITickSource
{
    TimeSpan CurrentInterval { get; }
    IDisposable Subscribe(Action onTick);
}
