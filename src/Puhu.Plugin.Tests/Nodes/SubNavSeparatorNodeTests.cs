using Puhu.Plugin.Nodes;
using Termina.Layout;
using Termina.Terminal;

namespace Puhu.Plugin.Tests.Nodes;

public sealed class SubNavSeparatorNodeTests
{
    [Fact]
    public void Measure_Returns_Height_1_And_Full_Width()
    {
        var sep = new SubNavSeparatorNode(Color.Cyan);
        var size = sep.Measure(new Size(80, 24));

        Assert.Equal(1, size.Height);
        Assert.Equal(80, size.Width);
    }
}
