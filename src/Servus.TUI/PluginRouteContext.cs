using Servus.Plugin;
using Termina.Hosting;
using Termina.Pages;

namespace Servus.TUI;

public sealed class PluginRouteContext(TerminaBuilder termina) : IRouteContext
{
    public void RegisterRoute<TPage, TViewModel>(string route)
        where TPage : class
        where TViewModel : class
    {
        var method = typeof(TerminaBuilder).GetMethods()
            .First(m => m.Name == "RegisterRoute" && m.GetGenericArguments().Length == 2)
            .MakeGenericMethod(typeof(TPage), typeof(TViewModel));
        method.Invoke(termina, [route, NavigationBehavior.PreserveState]);
    }
}
