using Puhu.Settings.Pages;
using Termina.Terminal;

namespace Puhu.Tests.Themes;

public sealed class ColorFormatTests
{
    [Fact]
    public void Rgb_FormatsAsLowercaseHex()
    {
        Assert.Equal("#0a0e14", ColorFormat.Describe(Color.FromHex("#0A0E14")));
    }

    [Fact]
    public void Indexed_FormatsAsIdx()
    {
        Assert.Equal("idx 11", ColorFormat.Describe(Color.FromIndex(11)));
    }

    [Fact]
    public void Default_FormatsAsDefault()
    {
        Assert.Equal("default", ColorFormat.Describe(Color.Default));
    }
}
