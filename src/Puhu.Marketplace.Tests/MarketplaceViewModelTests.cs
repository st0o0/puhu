using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Hosting;
using Puhu.Plugin.Marketplace;
using Puhu.Plugin.Marketplace.Actors;
using Puhu.Plugin.Marketplace.Models;
using Puhu.Plugin.Marketplace.Pages;

namespace Puhu.Marketplace.Tests;

public sealed class MarketplaceViewModelTests : IDisposable
{
    private readonly MarketplaceStore _store = new();
    private readonly MarketplaceViewModel _vm;

    public MarketplaceViewModelTests()
    {
        var registry = ActorRegistry.For(new ActorSystemImpl("TEST"));
        registry.Register<MarketplaceActor>(ActorRefs.Nobody);
        _vm = new MarketplaceViewModel(_store, registry);
    }

    [Fact]
    public void InstalledPlugins_FiltersOnlyInstalled()
    {
        _store.Update(new MarketplaceState
        {
            AvailablePlugins =
            [
                CreatePlugin("a", PluginStatus.Installed),
                CreatePlugin("b", PluginStatus.NotInstalled),
                CreatePlugin("c", PluginStatus.UpdateAvailable)
            ]
        });

        Assert.Equal(2, _vm.InstalledPlugins.Value.Count);
        Assert.Contains(_vm.InstalledPlugins.Value, p => p.Id == "a");
        Assert.Contains(_vm.InstalledPlugins.Value, p => p.Id == "c");
    }

    [Fact]
    public void Sources_ReflectsStoreState()
    {
        var sources = new PluginSources
        {
            Registries = ["https://reg.example.com"],
            Repositories = ["https://github.com/user/repo"]
        };
        _store.Update(new MarketplaceState { Sources = sources });

        Assert.Equal(sources, _vm.Sources.Value);
    }

    [Fact]
    public void ActiveOperations_ReflectsStoreState()
    {
        var ops = new Dictionary<string, string> { ["test"] = "Installing..." };
        _store.Update(new MarketplaceState { ActiveOperations = ops });

        Assert.True(_vm.ActiveOperations.Value.ContainsKey("test"));
    }

    [Fact]
    public void SwitchView_ResetsSelectedIndex()
    {
        _vm.MoveSelection(3);
        _vm.SwitchView(MarketplaceView.Installed);

        Assert.Equal(0, _vm.SelectedIndex.Value);
        Assert.Equal(MarketplaceView.Installed, _vm.ActiveView.Value);
    }

    public void Dispose()
    {
        _vm.Dispose();
        _store.Dispose();
    }

    private static PluginInfo CreatePlugin(string id, PluginStatus status) => new()
    {
        Id = id, Name = id, Description = "", Author = "", AvailableVersion = "1.0.0",
        Repository = "", Tags = [], Delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease },
        Status = status
    };
}
