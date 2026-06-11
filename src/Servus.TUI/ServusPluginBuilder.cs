using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Servus.TUI.Plugin;
using IActorContext = Servus.TUI.Plugin.IActorContext;

namespace Servus.TUI;

public sealed record ActorRegistrationInfo(string Name, Props Props, TimeSpan? MinInterval, bool AlwaysOn);

public sealed class ServusPluginBuilder(IServiceCollection services, ITickSource tickSource) : IServusPluginBuilder
{
    public IServiceCollection Services { get; } = services;
    public ITickSource TickSource { get; } = tickSource;
    public PluginTabInfo? Tab { get; private set; }
    public Action<IActorContext>? ActorSetup { get; private set; }
    public Action<IRouteContext>? RouteSetup { get; private set; }
    public Action<IServiceCollection>? ServiceSetup { get; private set; }
    public Action<IThemeContext>? ThemeSetup { get; private set; }
    public Action<INotificationContext>? NotificationSetup { get; private set; }
    public List<(string Key, Type Type)> SettingsRegistrations { get; } = [];
    public List<ActorRegistrationInfo> ActorRegistrations { get; } = [];

    public IServusPluginBuilder WithTab(string label, string route, ConsoleKey? hotKey = null)
    {
        Tab = new PluginTabInfo(label, route, hotKey);
        return this;
    }

    public IServusPluginBuilder ConfigureRoutes(Action<IRouteContext> configure)
    {
        RouteSetup = configure;
        return this;
    }

    public IServusPluginBuilder ConfigureActors(Action<IActorContext> configure)
    {
        ActorSetup = configure;
        return this;
    }

    public IServusPluginBuilder WithServices(Action<IServiceCollection> configure)
    {
        ServiceSetup = configure;
        return this;
    }

    public IServusPluginBuilder WithSettings<T>(string sectionKey) where T : class, new()
    {
        SettingsRegistrations.Add((sectionKey, typeof(T)));
        return this;
    }

    public IServusPluginBuilder WithTheme(Action<IThemeContext> configure)
    {
        ThemeSetup = configure;
        return this;
    }

    public IServusPluginBuilder WithNotifications(Action<INotificationContext> configure)
    {
        NotificationSetup = configure;
        return this;
    }
}
