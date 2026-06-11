namespace Servus.Plugin;

public interface IServusPlugin
{
    string Name { get; }
    void Configure(IServusPluginBuilder builder);
}
