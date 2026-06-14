using Puhu.Plugin;

namespace Puhu.Settings;

/// <summary>
/// First-run bookkeeping for the setup wizard. The runner reads
/// <see cref="IsComplete"/> to decide whether to show the wizard on startup;
/// the wizard calls <see cref="MarkComplete"/> when finished or skipped.
/// Versioned so a future release can re-onboard by bumping
/// <see cref="CurrentVersion"/>.
/// </summary>
public static class SetupWizardState
{
    public const string VersionKey = "puhu.setup-version";
    public const int CurrentVersion = 1;

    public static bool IsComplete(ISettingsStore settings) =>
        settings.Get<int?>(VersionKey) >= CurrentVersion;

    public static void MarkComplete(ISettingsStore settings) =>
        settings.Set(VersionKey, CurrentVersion);
}
