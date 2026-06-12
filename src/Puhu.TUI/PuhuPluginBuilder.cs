using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;
using IActorContext = Puhu.Plugin.IActorContext;

namespace Puhu.TUI;

public sealed record ActorRegistrationInfo(string Name, Props Props, TimeSpan? MinInterval, bool AlwaysOn);

public sealed class PuhuPluginBuilder(IServiceCollection services, ITickSource tickSource) : IPuhuPluginBuilder
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

    public IPuhuPluginBuilder WithTab(string label, string route, ConsoleKey? hotKey = null)
    {
        Tab = new PluginTabInfo(label, route, hotKey);
        return this;
    }

    public IPuhuPluginBuilder ConfigureRoutes(Action<IRouteContext> configure)
    {
        RouteSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder ConfigureActors(Action<IActorContext> configure)
    {
        ActorSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder WithServices(Action<IServiceCollection> configure)
    {
        ServiceSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder WithSettings<T>(string sectionKey) where T : class, new()
    {
        SettingsRegistrations.Add((sectionKey, typeof(T)));
        return this;
    }

    public IPuhuPluginBuilder WithTheme(Action<IThemeContext> configure)
    {
        ThemeSetup = configure;
        return this;
    }

    public IPuhuPluginBuilder WithNotifications(Action<INotificationContext> configure)
    {
        NotificationSetup = configure;
        return this;
    }
}
