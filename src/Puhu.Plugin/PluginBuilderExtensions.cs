using Termina.Reactive;

namespace Puhu.Plugin;

public static class PluginBuilderExtensions
{
    public static IPuhuPluginBuilder WithSettings<TPage, TViewModel>(
        this IPuhuPluginBuilder builder,
        string label)
        where TPage : ReactivePage<TViewModel>
        where TViewModel : ReactiveViewModel
    {
        var route = $"/settings/{label.ToLowerInvariant().Replace(' ', '-')}";

        builder.WithSettings(label, route);
        builder.WithRoutes(termina => termina.RegisterRoute<TPage, TViewModel>(route));

        return builder;
    }
}
