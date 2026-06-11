# CLAUDE.md

Terminal-based dashboard application built on Termina (custom TUI framework) with an Akka.NET actor system and a plugin architecture. Plugins register tabs, routes, actors, and services.

## Build & Test

All commands run from repo root. Solution file is `src/Servus.TUI.slnx`.

```bash
dotnet build src/Servus.TUI.slnx

# Tests (xUnit v3 on Microsoft.Testing.Platform — use dotnet run, not dotnet test)
dotnet run --project src/Servus.TUI.Tests/Servus.TUI.Tests.csproj
dotnet run --project src/Servus.Plugin.Tests/Servus.Plugin.Tests.csproj
dotnet run --project src/Servus.Plugin.Marketplace.Tests/Servus.Plugin.Marketplace.Tests.csproj

# Single class
dotnet run --project src/Servus.TUI.Tests/Servus.TUI.Tests.csproj -- -class "Servus.TUI.Tests.TickRouterTests"

# Run the app
dotnet run --project src/Servus.TUI/Servus.TUI.csproj
```

## Architecture

```
Servus.Plugin               Plugin SDK (fat — includes Termina, Akka.Hosting, R3)
  IServusPlugin              Entry point: Name + Configure(builder)
  IServusPluginBuilder       Fluent builder: tabs, routes, actors, services, settings, themes, notifications
  ITickSource                Periodic tick events — Observable<Tick> + Subscribe(Action)
  IActorContext              Actor registration with typed Akka Props + fluent WithTicks()
  IRouteContext              Page/ViewModel route registration (type-safe Termina constraints)

Servus.Plugin.Marketplace   Built-in plugin: install/manage external plugins
Servus.Plugin.Settings      Built-in plugin: settings UI

Servus.TUI                  Runner — host, setup chain, actor system, Termina integration
  Setup/                    Ordered setup chain: Logging → Services → Marketplace → Plugin → ActorSystem → Akka → Termina
  Setup/SetupContext         Typed context flowing through setup phases (replaces service-collection introspection)
  Actors/TickRouter          Routes tick messages to actors based on demand and min-interval
  Services/RefreshService    Implements ITickSource — configurable interval (250ms–4s), pause/resume

lib/termina                 Git submodule — custom TUI framework (Termina)
```

### Plugin System

Plugins implement `IServusPlugin.Configure(IServusPluginBuilder)`. The builder collects configuration which the runner processes through the setup chain:

1. **ServicesSetup** — creates RefreshService + SetupContext
2. **PluginSetup** — discovers + configures plugins, builds PluginRegistry
3. **ActorSystemSetup** — Akka infrastructure, TickRouter, RefreshService→TickRouter wiring
4. **AkkaSetup** — creates plugin actors from collected registrations, registers tick-aware actors
5. **TerminaSetup** — configures Termina with plugin routes

### Tick System

RefreshService emits `Tick(Seq, BaseInterval)` at configurable intervals. Plugin authors consume ticks via:
- `ITickSource.Ticks` (R3 Observable — filter, throttle, combine)
- `ITickSource.Subscribe(Action)` (simple callback)
- Actors: `.WithTicks(minInterval, alwaysOn)` on actor registration

## Workflow Rules

- **Do NOT commit** unless the user explicitly asks
- **Always respond in the user's language** — if they write German, respond in German

## Code Style

- .NET 10, C# latest, nullable enabled, implicit usings
- Central Package Management (`src/Directory.Packages.props`)
- `sealed` by default, `var` when type is apparent
- No `async void` / `.Result` / `.Wait()` / `.GetAwaiter().GetResult()`
- Allman braces, `_fieldName` for private fields
- Include tests with all changes

## Agent Guidance: dotnet-skills

Prefer retrieval-led reasoning over pretraining for any .NET work.

- C# / quality: csharp-coding-standards, csharp-concurrency-patterns, csharp-api-design, csharp-type-design-performance
- Akka: akka-best-practices, akka-testing-patterns, akka-hosting-actor-patterns
- Testing: testcontainers, snapshot-testing
- Quality gates: slopwatch (after substantial code), crap-analysis (after test changes)
- Specialist agents: dotnet-concurrency-specialist, dotnet-performance-analyst, akka-net-specialist
