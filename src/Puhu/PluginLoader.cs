using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;

namespace Puhu;

public static class PluginLoader
{
    public static PluginRegistry DiscoverAndConfigure(
        IServiceCollection services,
        ITickSource tickSource,
        IReadOnlyList<IPuhuPlugin> builtInPlugins)
    {
        var allPlugins = new List<IPuhuPlugin>(builtInPlugins);
        allPlugins.AddRange(DiscoverExternalPlugins());

        var builders = new List<PuhuPluginBuilder>();
        foreach (var plugin in allPlugins)
        {
            var builder = new PuhuPluginBuilder(services, tickSource);
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

    private static IReadOnlyList<IPuhuPlugin> DiscoverExternalPlugins()
    {
        var plugins = new List<IPuhuPlugin>();
        var scanDirs = GetPluginDirectories();

        foreach (var dir in scanDirs)
        {
            foreach (var dll in Directory.GetFiles(dir, "Puhu.Plugin.*.dll"))
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
        {
            yield return userDir;
        }

        var localDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (Directory.Exists(localDir))
        {
            yield return localDir;
        }
    }

    private static void DiscoverInAssembly(Assembly assembly, List<IPuhuPlugin> plugins)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPuhuPlugin).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
                {
                    plugins.Add((IPuhuPlugin)Activator.CreateInstance(type)!);
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
