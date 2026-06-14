using Puhu.Plugin;

namespace Puhu.Settings;

/// <summary>
/// First-run bookkeeping for the setup wizard. The runner reads
/// <see cref="IsWizardComplete"/> to decide whether to show the wizard on startup;
/// the wizard calls <see cref="MarkWizardComplete"/> when finished or skipped.
/// Versioned so a future release can re-onboard by bumping
/// <see cref="CurrentVersion"/>.
/// </summary>
public static class WizardStateExtensions
{
    internal const string VersionKey = "puhu.setup-version";
    internal const int CurrentVersion = 1;

    public static bool IsWizardComplete(this ISettingsStore settings)
        => settings.Get<int?>(VersionKey) >= CurrentVersion;

    public static void MarkWizardComplete(this ISettingsStore settings)
        => settings.Set(VersionKey, CurrentVersion);
}