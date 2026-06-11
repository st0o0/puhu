using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Termina.Pages;
using Termina.Reactive;

namespace Servus.TUI.Plugin;

/// <summary>
/// Fluent builder for configuring a Servus plugin — tabs, routes, actors, services, settings, themes, and notifications.
/// </summary>
public interface IServusPluginBuilder
{
    /// <summary>The host service collection for registering additional services.</summary>
    IServiceCollection Services { get; }

    /// <summary>The application tick source for subscribing to periodic refresh events.</summary>
    ITickSource TickSource { get; }

    /// <summary>Register a tab in the main tab bar.</summary>
    IServusPluginBuilder WithTab(string label, string route, ConsoleKey? hotKey = null);

    /// <summary>Register page routes for navigation.</summary>
    IServusPluginBuilder ConfigureRoutes(Action<IRouteContext> configure);

    /// <summary>Register Akka.NET actors for this plugin.</summary>
    IServusPluginBuilder ConfigureActors(Action<IActorContext> configure);

    /// <summary>Register additional services in the DI container.</summary>
    IServusPluginBuilder WithServices(Action<IServiceCollection> configure);

    /// <summary>Bind a settings section to a strongly-typed options class.</summary>
    IServusPluginBuilder WithSettings<T>(string sectionKey) where T : class, new();

    /// <summary>Register custom theme color keys.</summary>
    IServusPluginBuilder WithTheme(Action<IThemeContext> configure);

    /// <summary>Register notification types this plugin can emit.</summary>
    IServusPluginBuilder WithNotifications(Action<INotificationContext> configure);
}

/// <summary>
/// Context for registering page routes with their associated view models.
/// </summary>
public interface IRouteContext
{
    /// <summary>Register a page and view model pair for the given route.</summary>
    void RegisterRoute<TPage, TViewModel>(string route, NavigationBehavior? behavior = null)
        where TPage : ReactivePage<TViewModel>
        where TViewModel : ReactiveViewModel;
}

/// <summary>
/// Context for registering Akka.NET actors within a plugin.
/// </summary>
public interface IActorContext
{
    /// <summary>The application service provider for resolving dependencies.</summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>Register an actor with the given name and props. Returns a registration handle for optional tick configuration.</summary>
    IActorRegistration RegisterActor(string name, Props props);
}

/// <summary>
/// Fluent handle for configuring tick delivery to a registered actor.
/// </summary>
public interface IActorRegistration
{
    /// <summary>Subscribe this actor to periodic tick messages from the tick router.</summary>
    IActorRegistration WithTicks(TimeSpan? minInterval = null, bool alwaysOn = false);
}

/// <summary>
/// Context for registering custom theme color keys.
/// </summary>
public interface IThemeContext
{
    /// <summary>Register a named color key with a default hex value (e.g. "#FF5500").</summary>
    void RegisterColorKey(string key, string defaultHexColor);
}

/// <summary>
/// Context for registering notification types a plugin can emit.
/// </summary>
public interface INotificationContext
{
    /// <summary>Register a notification type with a display-friendly name.</summary>
    void RegisterNotificationType(string key, string displayName);
}
