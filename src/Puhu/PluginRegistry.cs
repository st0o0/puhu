using Puhu.Plugin;

namespace Puhu;

public sealed class PluginRegistry
{
    public IReadOnlyList<PuhuPluginBuilder> LoadedPlugins { get; }
    public IReadOnlyList<PluginTabInfo> PluginTabs { get; }
    public IReadOnlyList<PluginSettingsInfo> PluginSettings { get; }

    public PluginRegistry(IReadOnlyList<PuhuPluginBuilder> plugins)
    {
        LoadedPlugins = plugins;
        PluginTabs = plugins.Where(p => p.Tab is not null).Select(p => p.Tab!).ToList();
        PluginSettings = plugins.Where(p => p.Settings is not null).Select(p => p.Settings!).ToList();
    }
}
