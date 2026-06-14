using Puhu.Plugin;
using Puhu.Services;
using Puhu.Tests.Fakes;
using R3;

namespace Puhu.Tests.Services;

public sealed class RefreshControllerTests : IDisposable
{
    private readonly RefreshService _service;
    private readonly FakeSettingsStore _store = new();

    public RefreshControllerTests()
    {
        _service = new RefreshService(TimeSpan.FromMilliseconds(1000), _store);
    }

    public void Dispose() => _service.Dispose();

    [Fact]
    public void Steps_ExposesFiveSteps()
    {
        IRefreshController controller = _service;

        Assert.Equal(5, controller.Steps.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(250), controller.Steps[0]);
        Assert.Equal(TimeSpan.FromMilliseconds(4000), controller.Steps[^1]);
    }

    [Fact]
    public void SpeedUp_PersistsNewInterval()
    {
        IRefreshController controller = _service;

        controller.SpeedUp();

        Assert.Equal(TimeSpan.FromMilliseconds(500), controller.Interval.CurrentValue);
        Assert.Equal(500, _store.Get<int?>("puhu.refresh-interval"));
    }

    [Fact]
    public void SpeedUp_AtFastestStep_DoesNotPersistAgain()
    {
        IRefreshController controller = _service;
        controller.SetInterval(TimeSpan.FromMilliseconds(250));
        _store.SetCount = 0;

        controller.SpeedUp();

        Assert.Equal(0, _store.SetCount);
    }

    [Fact]
    public void SetInterval_SnapsToNearestStep()
    {
        IRefreshController controller = _service;

        controller.SetInterval(TimeSpan.FromMilliseconds(700));

        Assert.Equal(TimeSpan.FromMilliseconds(500), controller.Interval.CurrentValue);
        Assert.Equal(500, _store.Get<int?>("puhu.refresh-interval"));
    }

    [Fact]
    public void TogglePause_TogglesIsPaused()
    {
        IRefreshController controller = _service;

        controller.TogglePause();
        Assert.True(controller.IsPaused.CurrentValue);

        controller.TogglePause();
        Assert.False(controller.IsPaused.CurrentValue);
    }

    [Fact]
    public void TogglePause_DoesNotPersist()
    {
        IRefreshController controller = _service;
        _store.SetCount = 0;

        controller.TogglePause();

        Assert.Equal(0, _store.SetCount);
    }
}
