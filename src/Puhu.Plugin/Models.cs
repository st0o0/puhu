namespace Puhu.Plugin;

public sealed record PluginTabInfo(string Label, string Route);

public sealed record TabDescriptor(string Label, string Route);

public sealed record Tick(long Seq, TimeSpan BaseInterval);

public sealed record PluginSettingsInfo(string Label, string Route, string PluginName);
