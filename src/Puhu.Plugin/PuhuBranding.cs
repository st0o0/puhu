namespace Puhu.Plugin;

/// <summary>Shared Puhu branding assets, so the splash and the setup wizard use one logo and never drift.</summary>
internal static class PuhuBranding
{
    /// <summary>
    /// The Puhu wordmark (figlet) with the owl mascot, matching the README banner. Raw string literal so
    /// the backslashes and quotes are literal.
    /// </summary>
    private const string Logo = """
                                           ____        _
                                   ___    |  _ \ _   _| |__  _   _
                                  {o,o}   | |_) | | | | '_ \| | | |
                                  |)__)   |  __/| |_| | | | | |_| |
                                  -"-"-   |_|    \__,_|_| |_|\__,_|
                                """;

    /// <summary>
    /// The logo with every line right-padded to the widest line. Render THIS (not <see cref="Logo"/>)
    /// when centering: a multi-line <c>TextNode</c> centers each line by its own width, which shears the
    /// figlet unless every line shares one width.
    /// </summary>
    public static string LogoBlock { get; } = BuildBlock();

    private static string BuildBlock()
    {
        var lines = Logo.Replace("\r\n", "\n").Split('\n');
        var width = lines.Max(l => l.Length);
        return string.Join("\n", lines.Select(l => l.PadRight(width)));
    }
}
