# Puhu

Terminal-based dashboard application built on [Termina](https://github.com/Aaronontheweb/termina) with an Akka.NET actor system and a plugin architecture.

## Plugin SDK

Puhu is extensible through plugins. Install the SDK to build your own:

```bash
dotnet add package Puhu.Plugin
```

### Quick Start

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

### Sub-Navigation

Use `SubNavNode<TView>` for inline tab navigation within a page:

```csharp
var subNav = new SubNavNode<MyView>(
    ViewModel.ActiveView,
    KeyBindings,
    themeService,
    (ConsoleKey.B, "Browse", MyView.Browse),
    (ConsoleKey.S, "Search", MyView.Search));
```

## Build & Run

```bash
dotnet build src/Puhu.slnx
dotnet run --project src/Puhu/Puhu.csproj
```

## Tests

```bash
dotnet run --project src/Puhu.Plugin.Tests/Puhu.Plugin.Tests.csproj
dotnet run --project src/Puhu.Plugin.Api.Tests/Puhu.Plugin.Api.Tests.csproj
dotnet run --project src/Puhu.Tests/Puhu.Tests.csproj
dotnet run --project src/Puhu.Marketplace.Tests/Puhu.Marketplace.Tests.csproj
```

## License

MIT
