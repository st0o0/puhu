<div align="center">

```
╭─────────────────────────────────────╮
│           ____        _             │
│   ___    |  _ \ _   _| |__  _   _   │
│  {o,o}   | |_) | | | | '_ \| | | |  │
│  |)__)   |  __/| |_| | | | | |_| |  │
│  -"-"-   |_|    \__,_|_| |_|\__,_|  │
│                                     │
│  A terminal dashboard, made yours.  │
╰─────────────────────────────────────╯
```

A fast, themeable, plugin-driven TUI dashboard — built on [Termina](https://github.com/Aaronontheweb/termina) and an Akka.NET actor system, shipped as a single self-contained executable.

[![CI](https://github.com/st0o0/puhu/actions/workflows/ci.yml/badge.svg)](https://github.com/st0o0/puhu/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Puhu.Plugin.svg?label=Puhu.Plugin)](https://www.nuget.org/packages/Puhu.Plugin)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4.svg)](https://dotnet.microsoft.com/)

</div>

---

## What is Puhu?

Puhu is a terminal dashboard you build out of plugins. Each plugin contributes tabs, pages,
background actors, and settings; the runner wires them into a shared shell with a tab bar,
breadcrumb, status bar, and a configurable refresh loop. Out of the box it ships with a
**Marketplace** for installing more plugins and a **Settings** plugin with a first-run setup
wizard.

## Features

- **🦉 Single-file binary** — self-contained per-OS executable, no .NET runtime to install.
- **🧩 Plugin architecture** — register tabs, routes, Akka.NET actors, DI services, and settings through one fluent builder.
- **🎨 Themeable** — four built-in themes embedded in the binary, plus drop-in custom themes in the [btop](https://github.com/aristocratos/btop) theme format.
- **⚡ Tick-based refresh** — a shared refresh loop drives UI and actors; speed it up, slow it down, or pause it live.
- **🎬 First-run setup wizard** — pick a theme and get oriented on first launch.
- **🎭 Actor system** — Akka.NET under the hood, with tick-aware actor scheduling.

## Install

### Download a release (recommended)

Grab the archive for your platform from the [Releases](https://github.com/st0o0/puhu/releases)
page, extract it, and run the binary — it's fully self-contained, no .NET install required.

| Platform | Asset |
| --- | --- |
| Windows (x64) | `puhu-win-x64.zip` |
| Linux (x64) | `puhu-linux-x64.tar.gz` |
| macOS (Apple Silicon) | `puhu-osx-arm64.tar.gz` |

```bash
# Linux / macOS
tar -xzf puhu-linux-x64.tar.gz
./puhu-linux-x64/puhu
```

### Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/) (see [`src/global.json`](src/global.json))
and Git. Termina is a submodule, so clone recursively:

```bash
git clone --recursive https://github.com/st0o0/puhu.git
cd puhu
dotnet run --project src/Puhu/Puhu.csproj
```

## Usage

On first launch the setup wizard walks you through picking a theme. After that you land on the
dashboard. Global keys work from anywhere in the app:

| Key | Action |
| --- | --- |
| <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> | Next / previous tab |
| <kbd>+</kbd> / <kbd>-</kbd> | Faster / slower refresh |
| <kbd>P</kbd> | Pause / resume refresh |
| <kbd>Esc</kbd> | Quit |

Individual pages declare their own contextual key hints, shown in the shell's hint bar.

## Themes

Puhu ships with four themes baked into the binary:

- `btop-default` (default)
- `catppuccin-mocha`
- `gruvbox-dark`
- `tokyo-night`

### Custom themes

Themes use the **btop theme format** — plain `key="#rrggbb"` lines:

```ini
theme[main_bg]="#1e1e2e"
theme[main_fg]="#cdd6f4"
theme[selected_bg]="#89b4fa"
theme[hi_fg]="#f5c2e7"
theme[graph_start]="#a6e3a1"
theme[graph_mid]="#f9e2af"
theme[graph_end]="#f38ba8"
```

Drop a `.theme` file into either location and it shows up in the theme picker:

| Location | Use |
| --- | --- |
| `themes/` next to the executable | Portable / per-install themes |
| `~/.servus/themes/` | Per-user themes (survive updates) |

A custom theme that shares a name with a built-in **overrides** it, and the per-user folder
takes precedence over the portable one. Existing btop `.theme` files generally work as-is.

## Plugin SDK

Puhu is extensible through plugins. Install the SDK to build your own:

```bash
dotnet add package Puhu.Plugin
```

Implement `IPuhuPlugin` and configure everything through the fluent builder:

```csharp
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Puhu.Plugin;

public sealed class MyPlugin : IPuhuPlugin
{
    public string Name => "My Plugin";

    public void Configure(IPuhuPluginBuilder builder)
    {
        builder
            .WithTab("My Plugin", "/my-plugin")
            .WithServices(services => services.AddSingleton<MyService>())
            .WithActors((system, registry, resolver) =>
            {
                var actor = system.ActorOf(resolver.Props<MyActor>(), "my-actor");
                registry.Register<MyActor>(actor);
            })
            .WithRoutes(termina =>
                termina.RegisterRoute<MyPage, MyViewModel>("/my-plugin"));
    }
}
```

### Builder surface

| Method | Purpose |
| --- | --- |
| `WithTab(label, route)` | Add a tab to the main navigation |
| `WithRoutes(termina => …)` | Register page ⇄ view-model routes |
| `WithServices(services => …)` | Register services in the DI container |
| `WithActors((system, registry, resolver) => …)` | Create and register Akka.NET actors |
| `WithSettings(label, route)` | Add an entry to the Settings page (register its route via `WithRoutes`) |

### Settings

Read and write reactive, persisted settings from a ViewModel via a keyed `ISettingsStore`
(keyed by the plugin's name):

```csharp
var value = settings.Get<int>("refresh-interval");
settings.Set("refresh-interval", 30);
settings.Observe<int>("refresh-interval").Subscribe(v => /* react */);
```

### Ticks

Consume the shared refresh loop instead of running your own timer:

```csharp
// Simple callback
tickSource.Subscribe(() => /* refresh */);

// Or compose with R3 operators
tickSource.Ticks
    .Where(t => t.Seq % 4 == 0)
    .Subscribe(_ => /* every 4th tick */);
```

Actors can opt into ticks at registration time via the `RegisterMonitor` message (see the
built-in Marketplace plugin for a worked example).

### Sub-navigation

Use `SubNavNode<TView>` for inline tab navigation within a single page:

```csharp
var subNav = new SubNavNode<MyView>(
    ViewModel.ActiveView,
    KeyBindings,
    themeService,
    (ConsoleKey.B, "Browse", MyView.Browse),
    (ConsoleKey.S, "Search", MyView.Search));
```

## Architecture

```
Puhu.Plugin          Plugin SDK (Termina + Akka.Hosting + R3)
Puhu.Marketplace     Built-in plugin: install/manage external plugins
Puhu.Settings        Built-in plugin: settings UI + setup wizard
Puhu                 Runner — host, ordered setup chain, actor system, Termina integration
lib/termina          Git submodule: the Termina TUI framework
```

The runner processes plugin configuration through an ordered setup chain
(Logging → Services → Marketplace → Plugin → ActorSystem → Akka → Termina). A `RefreshService`
emits ticks at a configurable interval; a `TickRouter` fans them out to actors based on demand
and minimum interval. See [`CLAUDE.md`](CLAUDE.md) for the full design.

## Development

```bash
# Build everything
dotnet build src/Puhu.slnx

# Run the test suites (xUnit v3 on Microsoft.Testing.Platform — use `dotnet run`, not `dotnet test`)
dotnet run --project src/Puhu.Tests/Puhu.Tests.csproj
dotnet run --project src/Puhu.Plugin.Tests/Puhu.Plugin.Tests.csproj
dotnet run --project src/Puhu.Plugin.Api.Tests/Puhu.Plugin.Api.Tests.csproj
dotnet run --project src/Puhu.Marketplace.Tests/Puhu.Marketplace.Tests.csproj
```

Commits follow [Conventional Commits](https://www.conventionalcommits.org/) (enforced by
commitlint in CI), and releases are automated with release-please: a merge to `main` publishes
`Puhu.Plugin` to NuGet and attaches per-OS executables to a GitHub Release.

## License

[MIT](LICENSE)
