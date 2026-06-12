using Akka.Actor;
using R3;
using Puhu.Plugin.Marketplace.Models;
using Termina.Reactive;

namespace Puhu.Plugin.Marketplace.Pages;

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
    public ReactiveProperty<IReadOnlyList<PluginInfo>> InstalledPlugins { get; } = new([]);
    public ReactiveProperty<PluginSources> Sources { get; } = new(new PluginSources());
    public ReactiveProperty<IReadOnlyDictionary<string, string>> ActiveOperations { get; } =
        new(new Dictionary<string, string>());
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
                InstalledPlugins.Value = plugins
                    .Where(p => p.Status != PluginStatus.NotInstalled)
                    .ToList();
                UpdateSelectedPlugin();
            });

        store.State.Select(s => s.IsBusy).DistinctUntilChanged()
            .Subscribe(busy => IsSyncing.Value = busy);

        store.State.Select(s => s.StatusMessage).DistinctUntilChanged()
            .Subscribe(msg => StatusMessage.Value = msg);

        store.State.Select(s => s.Sources).DistinctUntilChanged()
            .Subscribe(sources => Sources.Value = sources);

        store.State.Select(s => s.ActiveOperations).DistinctUntilChanged()
            .Subscribe(ops => ActiveOperations.Value = ops);
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

    public void AddSource(string url) => _actor.Tell(new AddSource(url));

    public void RemoveSource(string url) => _actor.Tell(new RemoveSource(url));

    public void CyclePolicy(string pluginId) => _actor.Tell(new CycleUpdatePolicy(pluginId));

    public void UpdateSelected()
    {
        if (SelectedPlugin.Value is { Status: PluginStatus.UpdateAvailable } plugin)
        {
            _actor.Tell(new UpdatePlugin(plugin.Id));
        }
    }

    public void UninstallSelected()
    {
        if (SelectedPlugin.Value is { } plugin)
        {
            _actor.Tell(new UninstallPlugin(plugin.Id));
        }
    }

    public override void Dispose()
    {
        ActiveView.Dispose();
        AvailablePlugins.Dispose();
        InstalledPlugins.Dispose();
        Sources.Dispose();
        ActiveOperations.Dispose();
        SelectedIndex.Dispose();
        SelectedPlugin.Dispose();
        IsSyncing.Dispose();
        StatusMessage.Dispose();
        base.Dispose();
    }

    private void UpdateSelectedPlugin()
    {
        var list = GetCurrentList();
        if (SelectedIndex.Value >= list.Count)
        {
            SelectedIndex.Value = Math.Max(0, list.Count - 1);
        }

        SelectedPlugin.Value = list.Count > 0 ? list[SelectedIndex.Value] : null;
    }

    private IReadOnlyList<PluginInfo> GetCurrentList() => ActiveView.Value switch
    {
        MarketplaceView.Installed => InstalledPlugins.Value,
        _ => AvailablePlugins.Value
    };
}
