using System.Runtime.CompilerServices;

namespace Puhu.Plugin.Api.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.ScrubLinesContaining("[assembly: ReleaseDateAttribute(");
        UseProjectRelativeDirectory("verify");
        VerifierSettings.UniqueForRuntime();
        VerifierSettings.InitializePlugins();
    }
}