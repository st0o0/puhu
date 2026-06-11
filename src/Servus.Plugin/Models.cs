namespace Servus.Plugin;

/// <summary>Describes a tab registered by a plugin in the main tab bar.</summary>
/// <param name="Label">Display text shown in the tab bar.</param>
/// <param name="Route">Navigation route this tab activates.</param>
/// <param name="HotKey">Optional keyboard shortcut to switch to this tab.</param>
public sealed record PluginTabInfo(string Label, string Route, ConsoleKey? HotKey = null);

/// <summary>A periodic tick emitted by the refresh service.</summary>
/// <param name="Seq">Monotonically increasing sequence number (starts at 0).</param>
/// <param name="BaseInterval">The tick interval at the time this tick was generated.</param>
public sealed record Tick(long Seq, TimeSpan BaseInterval);
