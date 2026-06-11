using Akka.Actor;
using R3;
using Servus.Plugin.Marketplace.Models;
using Termina.Reactive;

namespace Servus.Plugin.Marketplace.Pages;

public enum MarketplaceView
{
    Browse,
    Installed,
    Sources
}

public sealed class MarketplaceViewModel : ReactiveViewModel
{
    private readonly IActorRef _actor;

    public ReactiveProperty<MarketplaceView> ActiveView { get; } = new(MarketplaceView.Browse);
    public ReactiveProperty<IReadOnlyList<PluginInfo>> AvailablePlugins { get; } = new([]);
    public ReactiveProperty<int> SelectedIndex { get; } = new(0);
    public ReactiveProperty<PluginInfo?> SelectedPlugin { get; } = new(null);
    public ReactiveProperty<bool> IsSyncing { get; } = new(false);
    public ReactiveProperty<string?> StatusMessage { get; } = new(null);

    public MarketplaceViewModel(MarketplaceStore store, IActorRef actor)
    {
        _actor = actor;

        store.State.Select(s => s.AvailablePlugins).DistinctUntilChanged()
            .Subscribe(plugins =>
            {
                AvailablePlugins.Value = plugins;
                UpdateSelectedPlugin();
            });

        store.State.Select(s => s.IsBusy).DistinctUntilChanged()
            .Subscribe(busy => IsSyncing.Value = busy);

        store.State.Select(s => s.StatusMessage).DistinctUntilChanged()
            .Subscribe(msg => StatusMessage.Value = msg);
    }

    public override void OnActivated() => _actor.Tell(new RefreshMarketplace());

    public void SwitchView(MarketplaceView view)
    {
        ActiveView.Value = view;
        SelectedIndex.Value = 0;
        UpdateSelectedPlugin();
    }

    public void MoveSelection(int delta)
    {
        SelectedIndex.Value = Math.Max(0, SelectedIndex.Value + delta);
        UpdateSelectedPlugin();
    }

    public void HandleAction()
    {
        if (SelectedPlugin.Value is not { } plugin)
        {
            return;
        }

        var message = plugin.Status switch
        {
            PluginStatus.NotInstalled => (object)new InstallPlugin(plugin.Id),
            PluginStatus.UpdateAvailable => new UpdatePlugin(plugin.Id),
            PluginStatus.Installed => new UninstallPlugin(plugin.Id),
            _ => null
        };

        if (message is not null)
        {
            _actor.Tell(message);
        }
    }

    public void Refresh() => _actor.Tell(new RefreshMarketplace());

    public void Sync() => _actor.Tell(new SyncAll());

    public override void Dispose()
    {
        ActiveView.Dispose();
        AvailablePlugins.Dispose();
        SelectedIndex.Dispose();
        SelectedPlugin.Dispose();
        IsSyncing.Dispose();
        StatusMessage.Dispose();
        base.Dispose();
    }

    private void UpdateSelectedPlugin()
    {
        var list = GetFilteredList();
        if (SelectedIndex.Value >= list.Count)
        {
            SelectedIndex.Value = Math.Max(0, list.Count - 1);
        }

        SelectedPlugin.Value = list.Count > 0 ? list[SelectedIndex.Value] : null;
    }

    private IReadOnlyList<PluginInfo> GetFilteredList() => ActiveView.Value == MarketplaceView.Installed
        ? AvailablePlugins.Value.Where(p => p.Status != PluginStatus.NotInstalled).ToList()
        : AvailablePlugins.Value;
}
