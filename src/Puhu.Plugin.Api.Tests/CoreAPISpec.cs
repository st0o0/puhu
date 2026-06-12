using PublicApiGenerator;

namespace Puhu.Plugin.Api.Tests;

public sealed class PluginAPISpec
{
    private static readonly ApiGeneratorOptions ApiOptions = new()
    {
        ExcludeAttributes =
        [
            "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute",
            "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
            "System.Runtime.CompilerServices.IteratorStateMachineAttribute"
        ]
    };

    private static Task VerifyAssembly<T>()
    {
        return Verify(typeof(T).Assembly.GeneratePublicApi(ApiOptions));
    }

    [Fact]
    public Task ApprovePlugin()
    {
        return VerifyAssembly<IPuhuPlugin>();
    }
}