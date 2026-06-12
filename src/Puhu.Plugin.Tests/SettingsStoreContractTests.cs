using System.Reflection;
using Puhu.Plugin;
using R3;

namespace Puhu.Plugin.Tests;

public sealed class SettingsStoreContractTests
{
    private static readonly Type StoreType = typeof(ISettingsStore);

    [Fact]
    public void Get_IsGenericMethod_ReturnsNullableT()
    {
        var method = StoreType.GetMethod("Get");
        Assert.NotNull(method);
        Assert.True(method.IsGenericMethodDefinition);
        Assert.Single(method.GetGenericArguments());
        Assert.Equal("key", method.GetParameters()[0].Name);
        Assert.Equal(typeof(string), method.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void Set_IsGenericMethod_ReturnsVoid()
    {
        var method = StoreType.GetMethod("Set");
        Assert.NotNull(method);
        Assert.True(method.IsGenericMethodDefinition);
        Assert.Single(method.GetGenericArguments());
        Assert.Equal(typeof(void), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal("key", parameters[0].Name);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal("value", parameters[1].Name);
    }

    [Fact]
    public void Observe_IsGenericMethod_ReturnsObservableOfT()
    {
        var method = StoreType.GetMethod("Observe");
        Assert.NotNull(method);
        Assert.True(method.IsGenericMethodDefinition);

        var typeArg = method.GetGenericArguments()[0];
        var expectedReturn = typeof(Observable<>).MakeGenericType(typeArg);
        Assert.Equal(expectedReturn, method.ReturnType);

        Assert.Equal("key", method.GetParameters()[0].Name);
        Assert.Equal(typeof(string), method.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void Remove_IsNotGeneric_ReturnsVoid()
    {
        var method = StoreType.GetMethod("Remove");
        Assert.NotNull(method);
        Assert.False(method.IsGenericMethodDefinition);
        Assert.Equal(typeof(void), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.Equal("key", parameters[0].Name);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
    }
}
