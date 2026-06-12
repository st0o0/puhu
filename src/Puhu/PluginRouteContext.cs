using Puhu.Plugin;
using Termina.Hosting;
using Termina.Pages;
using Termina.Reactive;

namespace Puhu;

public sealed class PluginRouteContext(TerminaBuilder termina) : IRouteContext
{
    public void RegisterRoute<TPage, TViewModel>(string route, NavigationBehavior? behavior = null)
        where TPage : ReactivePage<TViewModel>
        where TViewModel : ReactiveViewModel
    {
        termina.RegisterRoute<TPage, TViewModel>(route, behavior ?? NavigationBehavior.ResetOnNavigation);
    }
}