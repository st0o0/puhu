using Puhu.Plugin;

namespace Puhu.Setup;

public sealed class SetupContext
{
    public required ITickSource TickSource { get; init; }
    public PluginRegistry? PluginRegistry { get; set; }
}
