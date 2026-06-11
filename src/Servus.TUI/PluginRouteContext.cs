using Servus.Plugin;
using Termina.Hosting;
using Termina.Pages;
using Termina.Reactive;

namespace Servus.TUI;

public sealed class PluginRouteContext(TerminaBuilder termina) : IRouteContext
{
    public void RegisterRoute<TPage, TViewModel>(string route)
        where TPage : ReactivePage<TViewModel>
        where TViewModel : ReactiveViewModel
    {
        termina.RegisterRoute<TPage, TViewModel>(route, NavigationBehavior.PreserveState);
    }
}
