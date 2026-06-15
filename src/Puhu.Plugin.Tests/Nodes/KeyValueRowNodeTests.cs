using Puhu.Plugin.Nodes;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Tests.Nodes;

public sealed class KeyValueRowNodeTests
{
    [Fact]
    public void Measure_Returns_Height_1()
    {
        var row = new KeyValueRowNode("Author", "st0o0", labelWidth: 12,
            labelColor: Color.Gray, valueColor: Color.White);
        var size = row.Measure(new Size(80, 24));

        Assert.Equal(1, size.Height);
        Assert.Equal(80, size.Width);
    }
}
