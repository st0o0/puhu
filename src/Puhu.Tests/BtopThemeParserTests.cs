using Puhu.Themes;
using Termina.Terminal;

namespace Puhu.Tests;

public sealed class BtopThemeParserTests
{
    [Fact]
    public void Parse_GraphGradientKeys_BuildsGradientFromStops()
    {
        const string content = """
            theme[main_fg]="#cdd6f4"
            theme[graph_start]="#39d353"
            theme[graph_mid]="#ffea7f"
            theme[graph_end]="#ff7b72"
            """;

        var theme = BtopThemeParser.Parse(content);

        Assert.Equal(Color.FromHex("#39d353"), theme.GraphGradient.Sample(0f));
        Assert.Equal(Color.FromHex("#ff7b72"), theme.GraphGradient.Sample(1f));
    }

    [Fact]
    public void Parse_MissingGradientKeys_UsesDefaultGradient()
    {
        var theme = BtopThemeParser.Parse("""theme[main_fg]="#ffffff" """);

        Assert.Equal(Color.FromHex("#50fa7b"), theme.GraphGradient.Sample(0f));
        Assert.Equal(Color.FromHex("#ff5555"), theme.GraphGradient.Sample(1f));
    }
}
