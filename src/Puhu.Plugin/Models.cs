namespace Puhu.Plugin;

public sealed record PluginTabInfo(string Label, string Route);

public sealed record Tick(long Seq, TimeSpan BaseInterval);
