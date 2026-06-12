using Puhu.Plugin;

namespace Puhu.TUI;

public sealed class PluginRegistry
{
    public IReadOnlyList<PuhuPluginBuilder> LoadedPlugins { get; }
    public IReadOnlyList<PluginTabInfo> PluginTabs { get; }

    public PluginRegistry(IReadOnlyList<PuhuPluginBuilder> plugins)
    {
        LoadedPlugins = plugins;
        PluginTabs = plugins.Where(p => p.Tab is not null).Select(p => p.Tab!).ToList();
    }
}
