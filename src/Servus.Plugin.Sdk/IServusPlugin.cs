namespace Servus.Plugin.Sdk;

public interface IServusPlugin
{
    void Configure(IServusPluginBuilder builder);
}

public interface IServusPluginBuilder { }
