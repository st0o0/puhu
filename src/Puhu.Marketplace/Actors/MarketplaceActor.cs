using Akka.Actor;
using Puhu.Marketplace.Models;
using Puhu.Plugin;

namespace Puhu.Marketplace.Actors;

/// <summary>
/// Single owner of all marketplace state and operations.
/// Receives commands, delegates async I/O via PipeTo, pushes state into the store.
/// Tracks per-plugin active operations for UI feedback.
/// </summary>
public sealed class MarketplaceActor : ReceiveActor
{
    private MarketplaceState _state = new();

    public MarketplaceActor(IPluginManager pluginManager, MarketplaceStore store)
    {
        Receive<RefreshMarketplace>(_ =>
        {
            PushState(store, _state with { IsBusy = true });
            RefreshAllAsync(pluginManager)
                .PipeTo(Self,
                    success: result => result,
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<RefreshCompleted>(msg =>
        {
            PushState(store, _state with
            {
                AvailablePlugins = msg.Plugins,
                Sources = msg.Sources,
                IsBusy = false,
                StatusMessage = null
            });
        });

        Receive<InstallPlugin>(msg =>
        {
            SetActiveOp(store, msg.PluginId, "Installing...");
            pluginManager.InstallAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} installed"),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<UpdatePlugin>(msg =>
        {
            SetActiveOp(store, msg.PluginId, "Updating...");
            pluginManager.UpdateAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} updated"),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<UninstallPlugin>(msg =>
        {
            SetActiveOp(store, msg.PluginId, "Removing...");
            pluginManager.UninstallAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} removed"),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<SyncAll>(_ =>
        {
            PushState(store, _state with { IsBusy = true, StatusMessage = "Syncing..." });
            pluginManager.SyncAllAsync()
                .PipeTo(Self,
                    success: () => new RefreshMarketplace(),
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<OperationCompleted>(msg =>
        {
            ClearActiveOp(store, msg.PluginId, msg.Status);
            Self.Tell(new RefreshMarketplace());
        });

        Receive<OperationFailed>(msg =>
        {
            var ops = new Dictionary<string, string>(_state.ActiveOperations);
            ops.Remove(msg.PluginId);
            PushState(store, _state with
            {
                IsBusy = false,
                StatusMessage = $"Error: {msg.Error}",
                ActiveOperations = ops
            });
        });

        Receive<AddSource>(msg =>
        {
            pluginManager.AddSourceAsync(msg.Url, msg.Type)
                .PipeTo(Self,
                    success: () => new LoadSources(),
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<RemoveSource>(msg =>
        {
            pluginManager.RemoveSourceAsync(msg.Url)
                .PipeTo(Self,
                    success: () => new LoadSources(),
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<LoadSources>(_ =>
        {
            RefreshAllAsync(pluginManager)
                .PipeTo(Self,
                    success: result => result,
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<CycleUpdatePolicy>(msg =>
        {
            var plugin = _state.AvailablePlugins.FirstOrDefault(p => p.Id == msg.PluginId);
            if (plugin is null) return;

            var nextPolicy = plugin.UpdatePolicy switch
            {
                UpdatePolicy.Auto => UpdatePolicy.Manual,
                UpdatePolicy.Manual => UpdatePolicy.Pinned,
                UpdatePolicy.Pinned => UpdatePolicy.Auto,
                _ => UpdatePolicy.Auto
            };

            pluginManager.SetUpdatePolicyAsync(msg.PluginId, nextPolicy)
                .PipeTo(Self,
                    success: () => new RefreshMarketplace(),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<Tick>(_ => Self.Tell(new SyncAll()));
    }

    /// <summary>
    /// Seeds the default registry on first run, then loads both the current sources
    /// and the available plugins so the UI can render them together.
    /// </summary>
    private static async Task<RefreshCompleted> RefreshAllAsync(IPluginManager pluginManager)
    {
        await pluginManager.EnsureDefaultSourcesAsync();
        var sources = await pluginManager.GetSourcesAsync();
        var plugins = await pluginManager.FetchAvailableAsync();
        return new RefreshCompleted(plugins, sources);
    }

    private void SetActiveOp(MarketplaceStore store, string pluginId, string label)
    {
        var ops = new Dictionary<string, string>(_state.ActiveOperations) { [pluginId] = label };
        PushState(store, _state with { ActiveOperations = ops });
    }

    private void ClearActiveOp(MarketplaceStore store, string pluginId, string status)
    {
        var ops = new Dictionary<string, string>(_state.ActiveOperations);
        ops.Remove(pluginId);
        PushState(store, _state with
        {
            ActiveOperations = ops,
            StatusMessage = status
        });
    }

    private void PushState(MarketplaceStore store, MarketplaceState newState)
    {
        _state = newState;
        store.Update(newState);
    }
}
