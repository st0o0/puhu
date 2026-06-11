using Akka.Actor;
using Servus.Plugin.Marketplace.Models;
using Servus.TUI.Plugin;

namespace Servus.Plugin.Marketplace.Actors;

/// <summary>
/// Single owner of all marketplace state and operations.
/// Receives commands, delegates async I/O via PipeTo, pushes state into the store.
/// </summary>
public sealed class MarketplaceActor : ReceiveActor
{
    private MarketplaceState _state = new();

    public MarketplaceActor(IPluginManager pluginManager, MarketplaceStore store)
    {
        Receive<RefreshMarketplace>(_ =>
        {
            PushState(store, _state with { IsBusy = true });
            pluginManager.FetchAvailableAsync()
                .PipeTo(Self,
                    success: plugins => new RefreshCompleted(plugins),
                    failure: ex => new OperationFailed("", ex.Message));
        });

        Receive<RefreshCompleted>(msg =>
        {
            PushState(store, _state with
            {
                AvailablePlugins = msg.Plugins,
                IsBusy = false,
                StatusMessage = null
            });
        });

        Receive<InstallPlugin>(msg =>
        {
            PushState(store, _state with { IsBusy = true, StatusMessage = $"Installing {msg.PluginId}..." });
            pluginManager.InstallAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} installed. Restart to activate."),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<UpdatePlugin>(msg =>
        {
            PushState(store, _state with { IsBusy = true, StatusMessage = $"Updating {msg.PluginId}..." });
            pluginManager.UpdateAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} updated. Restart to activate."),
                    failure: ex => new OperationFailed(msg.PluginId, ex.Message));
        });

        Receive<UninstallPlugin>(msg =>
        {
            PushState(store, _state with { IsBusy = true, StatusMessage = $"Removing {msg.PluginId}..." });
            pluginManager.UninstallAsync(msg.PluginId)
                .PipeTo(Self,
                    success: () => new OperationCompleted(msg.PluginId, $"{msg.PluginId} removed. Restart to apply."),
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
            PushState(store, _state with { StatusMessage = msg.Status });
            Self.Tell(new RefreshMarketplace());
        });

        Receive<OperationFailed>(msg =>
        {
            PushState(store, _state with { IsBusy = false, StatusMessage = $"Error: {msg.Error}" });
        });

        Receive<Tick>(_ => Self.Tell(new SyncAll()));
    }

    private void PushState(MarketplaceStore store, MarketplaceState newState)
    {
        _state = newState;
        store.Update(newState);
    }
}
