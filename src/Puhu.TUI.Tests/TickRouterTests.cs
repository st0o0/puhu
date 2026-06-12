using Akka.Actor;
using Akka.Hosting;
using Akka.Hosting.TestKit;
using Puhu.TUI.Actors;
using Puhu.Plugin;

namespace Puhu.TUI.Tests;

public sealed class TickRouterTests : TestKit
{
    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
    }


    [Fact]
    public async Task AlwaysOn_Monitor_ReceivesTick()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("cpu", probe, AlwaysOn: true, MinInterval: null));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));

        await probe.ExpectMsgAsync<Tick>(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DemandZero_Monitor_DoesNotReceiveTick()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("disk", probe, AlwaysOn: false, MinInterval: null));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));

        await probe.ExpectNoMsgAsync(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PositiveDemand_Monitor_ReceivesTick()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("disk", probe, AlwaysOn: false, MinInterval: null));
        router.Tell(new DemandChanged("disk", +1));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));

        await probe.ExpectMsgAsync<Tick>(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DemandGoesBackToZero_Monitor_StopsReceiving()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("disk", probe, AlwaysOn: false, MinInterval: null));
        router.Tell(new DemandChanged("disk", +1));
        router.Tell(new DemandChanged("disk", -1));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));

        await probe.ExpectNoMsgAsync(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MinInterval_SkipsTick_WhenTooSoon()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("net", probe, AlwaysOn: true, MinInterval: TimeSpan.FromSeconds(2)));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));
        router.Tell(new Tick(1, TimeSpan.FromSeconds(1)));

        await probe.ExpectMsgAsync<Tick>(cancellationToken: TestContext.Current.CancellationToken);
        await probe.ExpectNoMsgAsync(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MinInterval_SendsTick_WhenEnoughTimeElapsed()
    {
        var router = Sys.ActorOf(Props.Create<TickRouter>());
        var probe = CreateTestProbe();

        router.Tell(new RegisterMonitor("net", probe, AlwaysOn: true, MinInterval: TimeSpan.FromSeconds(2)));
        router.Tell(new Tick(0, TimeSpan.FromSeconds(1)));
        router.Tell(new Tick(1, TimeSpan.FromSeconds(1)));
        router.Tell(new Tick(2, TimeSpan.FromSeconds(1)));

        var received = await probe.ReceiveNAsync(2, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken)
            .ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(2, received.Count);
    }
}