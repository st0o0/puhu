namespace Servus.Plugin.Sdk;

public interface IServusPlugin
{
    string Name { get; }
    void Configure(IServusPluginBuilder builder);
}
