# Puhu.Plugin

Plugin SDK for building [Puhu](https://github.com/st0o0/puhu) TUI dashboard plugins.

## Installation

```bash
dotnet add package Puhu.Plugin
```

## Quick Start

Create a plugin by implementing `IPuhuPlugin`:

```csharp
using Puhu.Plugin;

public sealed class MyPlugin : IPuhuPlugin
{
    public string Name => "My Plugin";

    public void Configure(IPuhuPluginBuilder builder)
    {
        builder
            .WithTab("My Plugin", "/my-plugin")
            .WithRoutes(termina =>
                termina.RegisterRoute<MyPage, MyViewModel>("/my-plugin"));
    }
}
```

## Features

### Tabs & Routes

Register a tab in the main navigation and a page route:

```csharp
builder
    .WithTab("Label", "/route")
    .WithRoutes(termina => termina.RegisterRoute<TPage, TViewModel>("/route"));
```

### Settings

Register a settings page with a reactive key-value store:

```csharp
builder.WithSettings<MySettingsPage, MySettingsViewModel>("My Plugin");
```

Access settings in your ViewModel via `[FromKeyedServices("my-plugin")] ISettingsStore`:

```csharp
var value = settings.Get<int>("refresh-interval");
settings.Set("refresh-interval", 30);
settings.Observe<int>("refresh-interval").Subscribe(v => /* react */);
```

### Actors

Register Akka.NET actors with optional tick-based scheduling:

```csharp
builder.WithActors((system, registry, resolver) =>
{
    var actor = system.ActorOf(resolver.Props<MyActor>(), "my-actor");
    registry.Register<MyActor>(actor);
});
```

### Services

Register services in the DI container:

```csharp
builder.WithServices(services => services.AddSingleton<MyService>());
```

## Sub-Navigation

Use `SubNavNode<TView>` for inline tab navigation within a page:

```csharp
var subNav = new SubNavNode<MyView>(
    ViewModel.ActiveView,
    KeyBindings,
    themeService,
    (ConsoleKey.B, "Browse", MyView.Browse),
    (ConsoleKey.S, "Search", MyView.Search));
```

## License

MIT
