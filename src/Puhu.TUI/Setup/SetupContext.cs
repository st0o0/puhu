using Puhu.Plugin;

namespace Puhu.TUI.Setup;

public sealed class SetupContext
{
    public required ITickSource TickSource { get; init; }
    public PluginRegistry? PluginRegistry { get; set; }
}
