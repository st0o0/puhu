using Akka.Actor;
using Akka.Hosting;
using Akka.Hosting.TestKit;
using Puhu.Plugin.Marketplace.Actors;
using Puhu.Plugin.Marketplace.Models;

namespace Puhu.Plugin.Marketplace.Tests;

public sealed class MarketplaceActorTests : TestKit
{
    private readonly FakePluginManager _pluginManager = new();
    private readonly MarketplaceStore _store = new();

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
    }

    [Fact]
    public async Task InstallPlugin_SetsActiveOperation()
    {
        var actor = Sys.ActorOf(Props.Create(() => new MarketplaceActor(_pluginManager, _store)));

        actor.Tell(new InstallPlugin("test-plugin"));

        await AwaitConditionAsync(
            () => _store.Current.ActiveOperations.ContainsKey("test-plugin"),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Installing...", _store.Current.ActiveOperations["test-plugin"]);
    }

    [Fact]
    public async Task OperationCompleted_ClearsActiveOperation()
    {
        _pluginManager.FetchResult = [];
        var actor = Sys.ActorOf(Props.Create(() => new MarketplaceActor(_pluginManager, _store)));

        actor.Tell(new InstallPlugin("test-plugin"));

        // Wait for install to set ActiveOperations
        await AwaitConditionAsync(
            () => _store.Current.ActiveOperations.ContainsKey("test-plugin"),
            cancellationToken: TestContext.Current.CancellationToken);

        // Complete the install
        _pluginManager.CompleteInstall();

        // After OperationCompleted, the active op should be cleared
        await AwaitConditionAsync(
            () => !_store.Current.ActiveOperations.ContainsKey("test-plugin"),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddSource_DelegatesToPluginManager()
    {
        var actor = Sys.ActorOf(Props.Create(() => new MarketplaceActor(_pluginManager, _store)));

        actor.Tell(new AddSource("https://github.com/test/repo"));

        await AwaitConditionAsync(
            () => _pluginManager.AddedSources.Contains("https://github.com/test/repo"),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CycleUpdatePolicy_CyclesAutoToManual()
    {
        var plugin = new PluginInfo
        {
            Id = "test-plugin",
            Name = "Test Plugin",
            Description = "A test plugin",
            Author = "test",
            AvailableVersion = "1.0.0",
            Repository = "https://github.com/test/plugin",
            Tags = [],
            Delivery = new PluginDelivery { Type = DeliveryType.GitHubRelease, Asset = "test.dll" },
            UpdatePolicy = UpdatePolicy.Auto,
            Status = PluginStatus.Installed,
            InstalledVersion = "1.0.0"
        };

        _pluginManager.FetchResult = [plugin];
        var actor = Sys.ActorOf(Props.Create(() => new MarketplaceActor(_pluginManager, _store)));

        // Populate state with a plugin that has Auto policy
        actor.Tell(new RefreshMarketplace());
        await AwaitConditionAsync(
            () => _store.Current.AvailablePlugins.Count == 1,
            cancellationToken: TestContext.Current.CancellationToken);

        // Cycle the policy
        actor.Tell(new CycleUpdatePolicy("test-plugin"));

        await AwaitConditionAsync(
            () => _pluginManager.LastSetPolicy == ("test-plugin", UpdatePolicy.Manual),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    private sealed class FakePluginManager : IPluginManager
    {
        private TaskCompletionSource _installTcs = new();

        public IReadOnlyList<PluginInfo> FetchResult { get; set; } = [];
        public List<string> AddedSources { get; } = [];
        public List<string> RemovedSources { get; } = [];
        public (string PluginId, UpdatePolicy Policy)? LastSetPolicy { get; private set; }

        public void CompleteInstall() => _installTcs.TrySetResult();

        public Task<IReadOnlyList<PluginInfo>> FetchAvailableAsync(bool ignoreCache = false)
            => Task.FromResult(FetchResult);

        public Task AddSourceAsync(string repoUrl)
        {
            AddedSources.Add(repoUrl);
            return Task.CompletedTask;
        }

        public Task RemoveSourceAsync(string repoUrl)
        {
            RemovedSources.Add(repoUrl);
            return Task.CompletedTask;
        }

        public Task InstallAsync(string pluginId) => _installTcs.Task;

        public Task UpdateAsync(string pluginId) => Task.CompletedTask;

        public Task UninstallAsync(string pluginId) => Task.CompletedTask;

        public Task SyncAllAsync() => Task.CompletedTask;

        public Task SetUpdatePolicyAsync(string pluginId, UpdatePolicy policy)
        {
            LastSetPolicy = (pluginId, policy);
            return Task.CompletedTask;
        }
    }
}
