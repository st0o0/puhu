using Puhu.Plugin;

namespace Puhu.Tests.Setup;

public sealed class PuhuBrandingTests
{
    [Fact]
    public void LogoBlock_AllLinesSameWidth()
    {
        var lines = PuhuBranding.LogoBlock.Replace("\r\n", "\n").Split('\n');

        Assert.True(lines.Length > 1);
        // every line padded to one width → centering keeps the figlet aligned
        Assert.Single(lines.Select(l => l.Length).Distinct());
    }
}
