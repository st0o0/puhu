using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Servus.Plugin.Sdk;

namespace Servus.Runner;

public static class PluginLoader
{
    public static PluginRegistry DiscoverAndConfigure(
        IServiceCollection services,
        ITickSource tickSource,
        IReadOnlyList<IServusPlugin> builtInPlugins)
    {
        var allPlugins = new List<IServusPlugin>(builtInPlugins);
        allPlugins.AddRange(DiscoverExternalPlugins());

        var builders = new List<ServusPluginBuilder>();
        foreach (var plugin in allPlugins)
        {
            var builder = new ServusPluginBuilder(services, tickSource);
            try
            {
                plugin.Configure(builder);
                builder.ServiceSetup?.Invoke(services);
                builders.Add(builder);
            }
            catch (Exception)
            {
                // Plugin failed to configure — skip it
            }
        }

        return new PluginRegistry(builders);
    }

    private static IReadOnlyList<IServusPlugin> DiscoverExternalPlugins()
    {
        var plugins = new List<IServusPlugin>();
        var scanDirs = GetPluginDirectories();

        foreach (var dir in scanDirs)
        {
            foreach (var dll in Directory.GetFiles(dir, "Servus.Plugin.*.dll"))
            {
                try
                {
                    var loadContext = new PluginLoadContext(dll);
                    var assembly = loadContext.LoadFromAssemblyPath(dll);
                    DiscoverInAssembly(assembly, plugins);
                }
                catch (Exception)
                {
                    // Failed to load assembly — skip it
                }
            }
        }

        return plugins;
    }

    private static IEnumerable<string> GetPluginDirectories()
    {
        var userDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".servus", "plugins");
        if (Directory.Exists(userDir))
            yield return userDir;

        var localDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (Directory.Exists(localDir))
            yield return localDir;
    }

    private static void DiscoverInAssembly(Assembly assembly, List<IServusPlugin> plugins)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IServusPlugin).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
                {
                    plugins.Add((IServusPlugin)Activator.CreateInstance(type)!);
                }
            }
        }
        catch (Exception)
        {
            // Failed to discover types — skip assembly
        }
    }
}

internal sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext
{
    private readonly string _pluginDir = Path.GetDirectoryName(pluginPath)!;

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var candidate = Path.Combine(_pluginDir, assemblyName.Name + ".dll");
        return File.Exists(candidate) ? LoadFromAssemblyPath(candidate) : null;
    }
}
