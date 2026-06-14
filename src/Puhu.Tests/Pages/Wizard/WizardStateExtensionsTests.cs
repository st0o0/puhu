using Puhu.Plugin;
using Puhu.Settings;
using Puhu.Tests.Fakes;

namespace Puhu.Tests.Pages.Wizard;

public sealed class WizardStateExtensionsTests
{
    [Fact]
    public void IsComplete_FalseWhenUnset()
    {
        Assert.False(new FakeSettingsStore().IsWizardComplete());
    }

    [Fact]
    public void IsComplete_TrueWhenVersionAtOrAboveCurrent()
    {
        var store = new FakeSettingsStore();
        store.Set(WizardStateExtensions.VersionKey, WizardStateExtensions.CurrentVersion);

        Assert.True(store.IsWizardComplete());
    }

    [Fact]
    public void MarkComplete_WritesCurrentVersion()
    {
        var store = new FakeSettingsStore();

        store.MarkWizardComplete();

        Assert.Equal(WizardStateExtensions.CurrentVersion, store.Get<int?>(WizardStateExtensions.VersionKey));
    }
}
