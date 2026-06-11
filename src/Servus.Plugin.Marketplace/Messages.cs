using Servus.Plugin.Marketplace.Models;

namespace Servus.Plugin.Marketplace;

// Commands — sent from ViewModel to actor
public sealed record RefreshMarketplace;
public sealed record InstallPlugin(string PluginId);
public sealed record UpdatePlugin(string PluginId);
public sealed record UninstallPlugin(string PluginId);
public sealed record SyncAll;

// Internal results — used by PipeTo inside the actor
internal sealed record RefreshCompleted(IReadOnlyList<PluginInfo> Plugins);
internal sealed record OperationCompleted(string PluginId, string Status);
internal sealed record OperationFailed(string PluginId, string Error);
