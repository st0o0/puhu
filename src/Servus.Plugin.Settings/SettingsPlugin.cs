using Servus.Plugin.Settings.Pages;
using Servus.Plugin;

namespace Servus.Plugin.Settings;

public sealed class SettingsPlugin : IServusPlugin
{
    public string Name => "Settings";

    public void Configure(IServusPluginBuilder builder)
    {
        builder
            .WithTab("Settings", "/settings")
            .ConfigureRoutes(ctx =>
                ctx.RegisterRoute<SettingsPage, SettingsViewModel>("/settings"));
    }
}
