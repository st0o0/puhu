using Termina.Terminal;

namespace Puhu.Settings.Pages;

/// <summary>Formats a <see cref="Color"/> for display in the theme palette.</summary>
internal static class ColorFormat
{
    public static string Describe(Color c) => c.Mode switch
    {
        ColorMode.Rgb => $"#{c.R:x2}{c.G:x2}{c.B:x2}",
        ColorMode.Indexed => $"idx {c.Index}",
        _ => "default",
    };
}
