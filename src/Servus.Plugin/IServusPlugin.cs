namespace Servus.Plugin;

/// <summary>
/// Entry point for a Servus plugin. Implement this interface and configure tabs, routes, actors, and services via the builder.
/// </summary>
public interface IServusPlugin
{
    /// <summary>Human-readable name of this plugin.</summary>
    string Name { get; }

    /// <summary>Configure the plugin using the provided builder.</summary>
    void Configure(IServusPluginBuilder builder);
}
