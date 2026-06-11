namespace Servus.Plugin.Sdk;

public sealed record PluginTabInfo(string Label, string Route, ConsoleKey? HotKey = null);
public sealed record Tick(long Seq, TimeSpan BaseInterval);
