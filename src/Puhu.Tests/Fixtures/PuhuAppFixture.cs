using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Puhu.Marketplace;
using Puhu.Marketplace.Actors;
using Puhu.Marketplace.Models;
using Puhu.Marketplace.Pages;
using Puhu.Pages;
using Puhu.Plugin;
using Puhu.Services;
using Puhu.Settings.Pages;
using Puhu.Setup;
using Puhu.Tests.Fakes;
using Puhu.Themes;
using Termina.Hosting;
using Termina.Input;
using Termina.Layout;
using Termina.Pages;
using Termina.Reactive;
using Termina.Terminal;

namespace Puhu.Tests.Fixtures;

/// <summary>
/// A fully wired Puhu host driven by Termina's <see cref="VirtualTerminal"/> and
/// <see cref="VirtualInputSource"/>, for end-to-end input → render tests. Mirrors the
/// pattern used in the dottop UI test suite: register the virtual terminal before
/// <c>AddTermina</c> (TryAdd skips it otherwise), feed keys through the virtual input
/// source, and assert against the rendered terminal buffer.
/// </summary>
public sealed class PuhuAppFixture : IAsyncDisposable
{
    public VirtualTerminal Terminal { get; }
    public VirtualInputSource Input { get; }

    // Exposed so tests can assert ViewModel-driven state (theme applied, paused, tab order)
    // in addition to the rendered screen.
    public IThemeService Theme { get; private set; } = null!;
    public RefreshService Refresh { get; private set; } = null!;
    public FakeTabOrderService TabOrder { get; private set; } = null!;
    public ISettingsStore Settings { get; private set; } = null!;
    public MarketplaceStore Marketplace { get; private set; } = null!;

    private IHost? _host;
    private string[] _tabRoutes = [];

    public PuhuAppFixture(int width = 80, int height = 24)
    {
        Terminal = new VirtualTerminal(width, height);
        Input = new VirtualInputSource();
    }

    /// <summary>
    /// Enable Tab/Shift+Tab cycling across the given routes (route 0 must be the start route).
    /// Call before <see cref="StartAsync"/>. Without it the app has no tabs and Tab is a no-op.
    /// </summary>
    public PuhuAppFixture WithTabs(params string[] routes)
    {
        _tabRoutes = routes;
        return this;
    }

    public async Task StartAsync(string startRoute = "/settings")
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;

        // Virtual terminal must be registered before AddTermina (TryAddSingleton).
        services.AddSingleton<IAnsiTerminal>(Terminal);

        Settings = new InMemorySettingsStore();
        services.AddSingleton(Settings);

        var theme = new ThemeService((InMemorySettingsStore)Settings);
        theme.LoadBuiltIns();
        theme.ApplyByName("btop-default");
        Theme = theme;
        services.AddSingleton<IThemeService>(theme);

        Refresh = new RefreshService(TimeSpan.FromSeconds(1), Settings);
        services.AddSingleton(Refresh);
        services.AddSingleton<IRefreshController>(Refresh);
        services.AddSingleton<ITickSource>(Refresh);

        TabOrder = new FakeTabOrderService(("System", "/system"), ("Marketplace", "/marketplace"));
        services.AddSingleton<ITabOrderService>(TabOrder);
        services.AddSingleton<ITabNavigator>(new CyclingTabNavigator(_tabRoutes));

        // Splash auto-navigates to the start route once its progress timer fills. A frozen
        // FakeTimeProvider keeps it on screen so its single key (Escape) can be tested.
        services.AddSingleton(new StartPageRoute("/system"));
        services.AddSingleton<TimeProvider>(new FakeTimeProvider());

        // Marketplace: seed the store and point the actor key at Nobody (so action keys are
        // routed and harmless) — no ActorSystem needed, ActorRegistry is a plain dictionary.
        Marketplace = new MarketplaceStore();
        Marketplace.Update(new MarketplaceState { AvailablePlugins = SeedPlugins() });
        services.AddSingleton(Marketplace);
        var actors = new ActorRegistry();
        actors.Register<MarketplaceActor>(ActorRefs.Nobody);
        services.AddSingleton<IReadOnlyActorRegistry>(actors);

        services.AddTerminaVirtualInput(Input);
        services.AddTermina(startRoute, termina =>
        {
            termina.RegisterRoute<SettingsPage, SettingsViewModel>("/settings");
            termina.RegisterRoute<SetupWizardPage, SetupWizardViewModel>("/setup");
            termina.RegisterRoute<SplashPage, SplashViewModel>("/splash");
            termina.RegisterRoute<MarketplacePage, MarketplaceViewModel>("/marketplace");
            termina.RegisterRoute<SystemStubPage, SystemStubViewModel>("/system");
        });

        _host = builder.Build();
        await _host.StartAsync();
    }

    /// <summary>Enqueue a key press. The app loop renders after processing it.</summary>
    public void SendKey(ConsoleKey key, bool shift = false, bool alt = false, bool control = false)
        => Input.EnqueueKey(key, shift, alt, control);

    /// <summary>
    /// Resolves once the app has begun shutting down (e.g. a quit key fired). Returns false on timeout.
    /// </summary>
    public async Task<bool> WaitForShutdownAsync(int timeoutMs = 5000)
    {
        var lifetime = _host!.Services.GetRequiredService<IHostApplicationLifetime>();
        var tcs = new TaskCompletionSource();
        await using var reg = lifetime.ApplicationStopping.Register(() => tcs.TrySetResult());
        if (lifetime.ApplicationStopping.IsCancellationRequested)
        {
            return true;
        }
        return await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) == tcs.Task;
    }

    public async ValueTask DisposeAsync()
    {
        Input.Complete();
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }
    }

    private static IReadOnlyList<PluginInfo> SeedPlugins() =>
    [
        NewPlugin("alpha", "Alpha Plugin", "First test plugin"),
        NewPlugin("beta", "Beta Plugin", "Second test plugin"),
    ];

    private static PluginInfo NewPlugin(string id, string name, string description) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Author = "tester",
        AvailableVersion = "1.0.0",
        Repository = "https://example.test/" + id,
        Tags = ["test"],
        Delivery = new PluginDelivery { Type = DeliveryType.NuGet, PackageId = id },
    };

    /// <summary>
    /// Cycles through a fixed route list, navigating on each Tab. Index 0 is the start route,
    /// so the first Tab moves to route 1 and Shift+Tab wraps to the last route.
    /// </summary>
    private sealed class CyclingTabNavigator(string[] routes) : ITabNavigator
    {
        private int _index;

        public bool HasTabs => routes.Length > 0;

        public void CycleTab(Action<string> navigate, int delta)
        {
            if (routes.Length == 0)
            {
                return;
            }
            _index = (_index + delta + routes.Length) % routes.Length;
            navigate(routes[_index]);
        }
    }
}

/// <summary>Minimal navigation target so wizard skip/complete has somewhere to land.</summary>
public sealed class SystemStubViewModel : ReactiveViewModel;

public sealed class SystemStubPage : ReactivePage<SystemStubViewModel>
{
    public override ILayoutNode BuildLayout() => new TextNode("system view");
}
