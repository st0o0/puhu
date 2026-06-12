using System.Text.Json;
using Puhu.Marketplace.Models;

namespace Puhu.Marketplace.Tests;

public sealed class RegistryIndexTests
{
    [Fact]
    public void Deserialize_ValidIndex()
    {
        var json = """
            {
              "version": 1,
              "plugins": [
                { "id": "servus.plugin.dtop", "repository": "https://github.com/st0o0/dtop" },
                { "id": "servus.plugin.docker", "repository": "https://github.com/st0o0/servus.plugin.docker" }
              ]
            }
            """;
        var index = JsonSerializer.Deserialize<RegistryIndex>(json)!;
        Assert.Equal(1, index.Version);
        Assert.Equal(2, index.Plugins.Count);
    }

    [Fact]
    public void Deserialize_EmptyPlugins()
    {
        var json = """{ "version": 1, "plugins": [] }""";
        var index = JsonSerializer.Deserialize<RegistryIndex>(json)!;
        Assert.Empty(index.Plugins);
    }
}
