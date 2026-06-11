using Microsoft.Extensions.DependencyInjection;

namespace Servus.Plugin;

public interface IServusPluginBuilder
{
    IServiceCollection Services { get; }
    ITickSource TickSource { get; }

    IServusPluginBuilder WithTab(string label, string route, ConsoleKey? hotKey = null);
    IServusPluginBuilder ConfigureRoutes(Action<IRouteContext> configure);
    IServusPluginBuilder ConfigureActors(Action<IActorContext> configure);
    IServusPluginBuilder WithServices(Action<IServiceCollection> configure);
    IServusPluginBuilder WithSettings<T>(string sectionKey) where T : class, new();
    IServusPluginBuilder WithTheme(Action<IThemeContext> configure);
    IServusPluginBuilder WithNotifications(Action<INotificationContext> configure);
}

public interface IRouteContext
{
    void RegisterRoute<TPage, TViewModel>(string route)
        where TPage : class
        where TViewModel : class;
}

public interface IActorContext
{
    IServiceProvider ServiceProvider { get; }
    ITickSource TickSource { get; }
    void RegisterActor(string name, Func<object> propsFactory);
}

public interface IThemeContext
{
    void RegisterColorKey(string key, string defaultHexColor);
}

public interface INotificationContext
{
    void RegisterNotificationType(string key, string displayName);
}
