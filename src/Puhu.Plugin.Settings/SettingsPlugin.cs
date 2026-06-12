using Puhu.Plugin.Settings.Pages;
using Puhu.Plugin;

namespace Puhu.Plugin.Settings;

public sealed class SettingsPlugin : IPuhuPlugin
{
    public string Name => "Settings";

    public void Configure(IPuhuPluginBuilder builder)
    {
        builder
            .WithTab("Settings", "/settings")
            .ConfigureRoutes(ctx =>
                ctx.RegisterRoute<SettingsPage, SettingsViewModel>("/settings"));
    }
}
