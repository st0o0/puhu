using Puhu.Plugin.Nodes;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Tests.Nodes;

public sealed class BadgeNodeTests
{
    [Fact]
    public void Measure_Returns_TextWidth_Plus_Padding()
    {
        var badge = new BadgeNode("ACTIVE", Color.Black, Color.Green);
        var size = badge.Measure(new Size(80, 1));

        // " ACTIVE " = 8 chars
        Assert.Equal(8, size.Width);
        Assert.Equal(1, size.Height);
    }

    [Fact]
    public void Measure_With_Icon_Includes_Icon_And_Space()
    {
        var badge = new BadgeNode("ACTIVE", Color.Black, Color.Green, icon: "●");
        var size = badge.Measure(new Size(80, 1));

        // " ● ACTIVE " = 10 chars
        Assert.Equal(10, size.Width);
        Assert.Equal(1, size.Height);
    }
}
