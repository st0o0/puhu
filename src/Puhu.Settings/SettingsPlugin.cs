using Puhu.Plugin;
using Puhu.Settings.Pages;

namespace Puhu.Settings;

public sealed class SettingsPlugin : IPuhuPlugin
{
    public string Name => "Settings";

    public void Configure(IPuhuPluginBuilder builder)
    {
        builder
            .WithTab("Settings", "/settings")
            .WithRoutes(termina =>
                termina.RegisterRoute<SettingsPage, SettingsViewModel>("/settings"));
    }
}
