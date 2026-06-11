using Servus.TUI.Plugin;

namespace Servus.TUI;

public sealed class PluginRegistry
{
    public IReadOnlyList<ServusPluginBuilder> LoadedPlugins { get; }
    public IReadOnlyList<PluginTabInfo> PluginTabs { get; }

    public PluginRegistry(IReadOnlyList<ServusPluginBuilder> plugins)
    {
        LoadedPlugins = plugins;
        PluginTabs = plugins.Where(p => p.Tab is not null).Select(p => p.Tab!).ToList();
    }
}
