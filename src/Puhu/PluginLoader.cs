using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;
using Puhu.Services;

namespace Puhu;

public static class PluginLoader
{
    public static PluginRegistry DiscoverAndConfigure(
        IServiceCollection services,
        IReadOnlyList<IPuhuPlugin> builtInPlugins)
    {
        var allPlugins = new List<IPuhuPlugin>(builtInPlugins);
        allPlugins.AddRange(DiscoverExternalPlugins());

        var builders = new List<PuhuPluginBuilder>();
        foreach (var plugin in allPlugins)
        {
            var pluginName = ToKebabCase(plugin.Name);
            var builder = new PuhuPluginBuilder(services, pluginName);
            try
            {
                plugin.Configure(builder);
                builder.ServiceSetup?.Invoke(services);
                builders.Add(builder);

                services.AddKeyedSingleton<ISettingsStore>(pluginName, (sp, _) =>
                    new ScopedSettingsStore(sp.GetRequiredService<SettingsStore>(), pluginName));
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
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
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
            foreach (var subDir in Directory.GetDirectories(userDir))
                yield return subDir;
        }

        var localDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        if (Directory.Exists(localDir))
        {
            yield return localDir;
            foreach (var subDir in Directory.GetDirectories(localDir))
                yield return subDir;
        }
    }

    private static string ToKebabCase(string name)
    {
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (c is ' ' or '_')
            {
                result.Append('-');
            }
            else if (char.IsUpper(c) && i > 0 && name[i - 1] != ' ' && name[i - 1] != '_' && !char.IsUpper(name[i - 1]))
            {
                result.Append('-');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(char.ToLowerInvariant(c));
            }
        }
        return result.ToString();
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
