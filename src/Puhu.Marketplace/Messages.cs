using Puhu.Marketplace.Models;

namespace Puhu.Marketplace;

// Commands — sent from ViewModel to actor
public sealed record RefreshMarketplace;
public sealed record InstallPlugin(string PluginId);
public sealed record UpdatePlugin(string PluginId);
public sealed record UninstallPlugin(string PluginId);
public sealed record SyncAll;

// Internal results — used by PipeTo inside the actor
internal sealed record RefreshCompleted(IReadOnlyList<PluginInfo> Plugins, PluginSources Sources);
internal sealed record OperationCompleted(string PluginId, string Status);
internal sealed record OperationFailed(string PluginId, string Error);

// Source management commands
public enum SourceType { Registry, Repository }
public sealed record AddSource(string Url, SourceType Type);
public sealed record RemoveSource(string Url);
public sealed record CycleUpdatePolicy(string PluginId);
public sealed record LoadSources;
