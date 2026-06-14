# CLAUDE.md

Terminal-based dashboard application built on Termina (custom TUI framework) with an Akka.NET actor system and a plugin architecture. Plugins register tabs, routes, actors, and services.

## Build & Test

All commands run from repo root. Solution file is `src/Puhu.slnx`.

```bash
dotnet build src/Puhu.slnx

# Tests (xUnit v3 on Microsoft.Testing.Platform — use dotnet run, not dotnet test)
dotnet run --project src/Puhu.Tests/Puhu.Tests.csproj
dotnet run --project src/Puhu.Plugin.Tests/Puhu.Plugin.Tests.csproj
dotnet run --project src/Puhu.Marketplace.Tests/Puhu.Marketplace.Tests.csproj

# Single class
dotnet run --project src/Puhu.Tests/Puhu.Tests.csproj -- -class "Puhu.Tests.TickRouterTests"

# Run the app
dotnet run --project src/Puhu/Puhu.csproj
```

## Architecture

```
Puhu.Plugin                 Plugin SDK (includes Termina, Akka.Hosting, R3)
  IPuhuPlugin               Entry point: Name + Configure(builder)
  IPuhuPluginBuilder         Fluent builder: tabs, routes, actors, services, settings, themes, notifications
  ITickSource                Periodic tick events — Observable<Tick> + Subscribe(Action)
  IKeyHintProvider           Pages implement this to declare key hints; runner wraps in shell chrome
  ITabNavigator              Tab-cycling abstraction — plugins use it, runner implements it
  SubNavNode<T>              Reusable sub-navigation node for plugin pages

Puhu.Marketplace            Built-in plugin: install/manage external plugins
Puhu.Settings               Built-in plugin: settings UI

Puhu                        Runner — host, setup chain, actor system, Termina integration
  Setup/                    Ordered setup chain: Logging → Services → Marketplace → Plugin → ActorSystem → Akka → Termina
  Setup/SetupContext         Typed context flowing through setup phases (replaces service-collection introspection)
  Nodes/                    Shell chrome (AppShellNode, TabBarNode, KeyHintsNode, SeparatorNode) — internal to runner
  Actors/TickRouter          Routes tick messages to actors based on demand and min-interval
  Services/RefreshService    Implements ITickSource — configurable interval (250ms–4s), pause/resume

lib/termina                 Git submodule — custom TUI framework (Termina)
```

### Plugin System

Plugins implement `IPuhuPlugin.Configure(IPuhuPluginBuilder)`. The builder collects configuration which the runner processes through the setup chain:

1. **ServicesSetup** — creates RefreshService + SetupContext
2. **PluginSetup** — discovers + configures plugins, builds PluginRegistry
3. **ActorSystemSetup** — Akka infrastructure, TickRouter, RefreshService→TickRouter wiring
4. **AkkaSetup** — creates plugin actors from collected registrations, registers tick-aware actors
5. **TerminaSetup** — configures Termina with plugin routes + registers shell LayoutDecorator (wraps IKeyHintProvider pages)

### Tick System

RefreshService emits `Tick(Seq, BaseInterval)` at configurable intervals. Plugin authors consume ticks via:
- `ITickSource.Ticks` (R3 Observable — filter, throttle, combine)
- `ITickSource.Subscribe(Action)` (simple callback)
- Actors: `.WithTicks(minInterval, alwaysOn)` on actor registration

## Workflow Rules

- **Do NOT commit** unless the user explicitly asks
- **Always work in English** — all chat responses, code, comments, UI strings, and commit messages in English, even when the user writes in another language

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
