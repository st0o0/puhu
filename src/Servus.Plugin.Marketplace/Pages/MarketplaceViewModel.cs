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
    private readonly IPluginManager _pluginManager;

    public ReactiveProperty<MarketplaceView> ActiveView { get; } = new(MarketplaceView.Browse);
    public ReactiveProperty<IReadOnlyList<PluginInfo>> AvailablePlugins { get; } = new([]);
    public ReactiveProperty<int> SelectedIndex { get; } = new(0);
    public ReactiveProperty<PluginInfo?> SelectedPlugin { get; } = new(null);
    public ReactiveProperty<bool> IsSyncing { get; } = new(false);
    public ReactiveProperty<string?> StatusMessage { get; } = new(null);

    public MarketplaceViewModel(IPluginManager pluginManager) => _pluginManager = pluginManager;

    public override void OnActivated() => _ = LoadDataAsync();

    public async Task LoadDataAsync()
    {
        try
        {
            IsSyncing.Value = true;
            AvailablePlugins.Value = await _pluginManager.FetchAvailableAsync();
            UpdateSelectedPlugin();
        }
        finally
        {
            IsSyncing.Value = false;
        }
    }

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

    public async Task HandleActionAsync()
    {
        if (SelectedPlugin.Value is not { } plugin)
        {
            return;
        }

        try
        {
            IsSyncing.Value = true;
            switch (plugin.Status)
            {
                case PluginStatus.NotInstalled:
                    StatusMessage.Value = $"Installing {plugin.Name}...";
                    await _pluginManager.InstallAsync(plugin.Id);
                    StatusMessage.Value = $"{plugin.Name} installed. Restart to activate.";
                    break;
                case PluginStatus.UpdateAvailable:
                    StatusMessage.Value = $"Updating {plugin.Name}...";
                    await _pluginManager.UpdateAsync(plugin.Id);
                    StatusMessage.Value = $"{plugin.Name} updated. Restart to activate.";
                    break;
                case PluginStatus.Installed:
                    StatusMessage.Value = $"Removing {plugin.Name}...";
                    await _pluginManager.UninstallAsync(plugin.Id);
                    StatusMessage.Value = $"{plugin.Name} removed. Restart to apply.";
                    break;
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage.Value = $"Error: {ex.Message}";
        }
        finally
        {
            IsSyncing.Value = false;
        }
    }

    public async Task SyncAsync()
    {
        try
        {
            IsSyncing.Value = true;
            StatusMessage.Value = "Syncing...";
            await _pluginManager.SyncAllAsync();
            await LoadDataAsync();
            StatusMessage.Value = "Sync complete.";
        }
        catch (Exception ex)
        {
            StatusMessage.Value = $"Sync error: {ex.Message}";
        }
        finally
        {
            IsSyncing.Value = false;
        }
    }

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