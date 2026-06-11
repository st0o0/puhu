using Servus.TUI.Plugin;

namespace Servus.TUI.Setup;

public sealed class SetupContext
{
    public required ITickSource TickSource { get; init; }
    public PluginRegistry? PluginRegistry { get; set; }
}
