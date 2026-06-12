# Changelog

## [0.2.0](https://github.com/st0o0/puhu/compare/v0.1.0...v0.2.0) (2026-06-12)


### Features

* add app shell, IThemeService, and UI chrome components ([65e28ca](https://github.com/st0o0/puhu/commit/65e28ca39fd3b4441988c0208a814a352cdceaf4))
* add btop theming support and ESC exit on splash page ([014b1de](https://github.com/st0o0/puhu/commit/014b1de629360705ed77647f428b02fb6862f344))
* add ISettingsStore interface to plugin SDK ([77ac29f](https://github.com/st0o0/puhu/commit/77ac29f74b5984ae55ef2c771f654796d019b434))
* add marketplace domain models with JSON serialization and status merging ([bd09ade](https://github.com/st0o0/puhu/commit/bd09ade0fa6d146dfa528a6a8d96aad5ca4facb0))
* add MarketplacePlugin with page, view model, and route registration ([fd092ae](https://github.com/st0o0/puhu/commit/fd092aecb59094407423c01732dcad1e480c2ec1))
* add PluginCache, PluginMetadataFetcher, and PluginDownloader ([abeae3a](https://github.com/st0o0/puhu/commit/abeae3a27f1b370ba87989853621e8e8bc5271d8))
* add PluginConfigStore for sources.json and installed.json with default seeding ([8c36231](https://github.com/st0o0/puhu/commit/8c362311bfcf8320838a2bab6438a2706587ccb3))
* add PluginLoader with built-in + external plugin discovery ([614e9ab](https://github.com/st0o0/puhu/commit/614e9abbdf9221d18f6f0622d85da63599dbe6fd))
* add PluginManager orchestrating install, update, uninstall, sync ([1ee4989](https://github.com/st0o0/puhu/commit/1ee4989dab80601cfa9f9e97038ac893557809d0))
* add PluginSettingsInfo record ([6bc8120](https://github.com/st0o0/puhu/commit/6bc8120ae551ace09fd40567fa70477cb55def8f))
* Add public API verification tests ([d7c3b87](https://github.com/st0o0/puhu/commit/d7c3b872c4707445a5e102b8d27bbfc6be93300e))
* add RefreshService with interval snapping, speed up/down, and tick emission ([29426f5](https://github.com/st0o0/puhu/commit/29426f5f06d9c6eca458e5818dee0479ff2ae860))
* add Servus.Plugin.Sdk with plugin contracts, tick source, and models ([e186835](https://github.com/st0o0/puhu/commit/e186835c65d07763cbcceaaf9cbe909982522378))
* add ServusPluginBuilder and PluginRegistry for plugin configuration ([57fd96f](https://github.com/st0o0/puhu/commit/57fd96f8ad03139c74c8451c1d75dd49f6a6fd13))
* add SettingsPlugin with placeholder page and route registration ([b280ba0](https://github.com/st0o0/puhu/commit/b280ba018542252cdeb3654cfacdd9516d2d6ee1))
* add setup chain, tab bar, plugin routing, and Program.cs entry point ([aa557ee](https://github.com/st0o0/puhu/commit/aa557ee761d0ed418e05c530a61d268e4ebe5f26))
* add SubNavNode&lt;TView&gt; with inline tab rendering ([b5df6b7](https://github.com/st0o0/puhu/commit/b5df6b7de32cee5a259351bec3343eaeb6e14d4f))
* add TickRouter actor with demand-driven tick distribution ([f59bb5d](https://github.com/st0o0/puhu/commit/f59bb5d91f91ac824790789abba7160fef4ed68e))
* add WithSettings to plugin builder API ([31d884e](https://github.com/st0o0/puhu/commit/31d884e9e2ba907feb4784cb68944aab79ff545e))
* add WithSettings&lt;TPage, TViewModel&gt; extension method ([86efa80](https://github.com/st0o0/puhu/commit/86efa803a169bd1e816cc33513d60cc479348fee))
* Allow custom navigation behavior for routes ([84ef1fb](https://github.com/st0o0/puhu/commit/84ef1fb6e659a5f26081805252e4b3c311f4b96a))
* collect PluginSettings in PluginRegistry ([a7a194a](https://github.com/st0o0/puhu/commit/a7a194a550eec49b2643b193036ffa2e4f595d3f))
* expose Observable&lt;Tick&gt; on ITickSource for reactive tick consumption ([e3d8605](https://github.com/st0o0/puhu/commit/e3d8605f6ceaf927698b2ade7634b8f467f2e18e))
* implement ScopedSettingsStore with plugin-name prefix ([579d66d](https://github.com/st0o0/puhu/commit/579d66d22c6189d0e1e57907ec10b348a647ea79))
* implement SettingsStore with JSON persistence ([155faef](https://github.com/st0o0/puhu/commit/155faefda3273b1077a4ded30f7cb923a8ac3531))
* Introduce Akka actor for marketplace management ([382bb78](https://github.com/st0o0/puhu/commit/382bb78334f9eeddacac48dada1e6fe78de20457))
* **marketplace:** actor handles AddSource, RemoveSource, CycleUpdatePolicy and per-plugin ops ([3f30e69](https://github.com/st0o0/puhu/commit/3f30e69d66216a063ccfd4e36b73df1e2d967241))
* **marketplace:** extend state with ActiveOperations, Sources and new messages ([61ead77](https://github.com/st0o0/puhu/commit/61ead7732360c532795b2298f974e2978530c650))
* **marketplace:** implement Browse, Installed, Sources views with keybindings and key hints ([5150390](https://github.com/st0o0/puhu/commit/51503904afd85dab03d78322e381d97cb2b0164a))
* **marketplace:** page skeleton with tab bar, view switching, and toast integration ([2a9e791](https://github.com/st0o0/puhu/commit/2a9e791718371101bd8b5cefa94a3e300ed0e118))
* migrate Marketplace to SubNavNode, replace D1/D2/D3 with B/I/S ([433428f](https://github.com/st0o0/puhu/commit/433428f381c3ed2890dcf9d349f8d054b80c0e02))
* pass plugin name to builder, kebab-case conversion ([2fc5308](https://github.com/st0o0/puhu/commit/2fc53085ac4bdb8535d53e5397304ecb3985433c))
* rebrand to Puhu naming and add release-please flow ([b390869](https://github.com/st0o0/puhu/commit/b39086987511802939501db542ce0e4691d20c42))
* register keyed ISettingsStore per plugin ([dfdaceb](https://github.com/st0o0/puhu/commit/dfdaceb83ae6f5726aa2507b17dad752f7ef2023))
* register SettingsStore singleton in ServicesSetup ([cf075a1](https://github.com/st0o0/puhu/commit/cf075a17c84cba276b2e50eff5cd0a69556b980d))
* **splash:** add SplashPage with ASCII logo and progress bar ([2540281](https://github.com/st0o0/puhu/commit/254028131c8b642a1cfa89b595f5b9c7716bfdbf))
* **splash:** add SplashViewModel with animated progress ([35e7437](https://github.com/st0o0/puhu/commit/35e74371609a3bdc63fbeb3ec10ab34f351f1f2a))
* **splash:** wire splash page into startup, navigate via ViewModel ([64e1337](https://github.com/st0o0/puhu/commit/64e13378a8dabc138bd0d94700c4f6c659c22ca9))
* type-safe route registration and XML summaries on public API ([526a5d9](https://github.com/st0o0/puhu/commit/526a5d90cc512d8b8ecc6adeb21d8444c894f875))
* typed IActorContext with Akka Props and fluent IActorRegistration ([5a9f9da](https://github.com/st0o0/puhu/commit/5a9f9da1b47e021c7ad3572d81a77c12626712b7))
* **ui:** Implement key hint and tab navigation interfaces ([e02a92b](https://github.com/st0o0/puhu/commit/e02a92b05214f119105387ae17aa46510502f9c3))


### Bug Fixes

* block Tab on splash, update termina submodule with dispose fix ([0ce68e3](https://github.com/st0o0/puhu/commit/0ce68e32a2a5193294fda98154f5d140cb6fac15))
* remove double-dispose of ProgressBarNode in SplashPage ([bc5db79](https://github.com/st0o0/puhu/commit/bc5db7928115d0b6a22c854f2ef2b6e24430cb1c))
* render border lines as strings to avoid DiffingTerminal char-width gaps ([4f4def3](https://github.com/st0o0/puhu/commit/4f4def35362bef62bfd21df65b7fd10fd8ad02ed))
* replace PanelNode+SeparatorNode with custom AppShellNode ([f20f982](https://github.com/st0o0/puhu/commit/f20f98293ccfad16d5e4efb9f84857ce38f96fc6))
* Shift+Tab navigation via page keybindings + Termina backtab fix ([2ca06f9](https://github.com/st0o0/puhu/commit/2ca06f972eaf12af6d61bb00405efc6d3cef7e24))
* **startup:** resolve actor registry and startup order for plugin ViewModels ([90467db](https://github.com/st0o0/puhu/commit/90467dbd8dcdf025b3b7c6364fdff55dbda1d58b))
* stop splash animation on deactivation to prevent ObjectDisposedException ([517ac89](https://github.com/st0o0/puhu/commit/517ac89594543d71f070a7f64c2bb5e88325eccd))


### Refactoring

* extract AkkaSetup for plugin actor registration, simplify ActorSystemSetup ([8e87410](https://github.com/st0o0/puhu/commit/8e874108a5b3695852d7adbe33a770cdfe51dbf2))
* global Tab/Shift+Tab navigation, eliminate ThemeService.Instance ([75fff45](https://github.com/st0o0/puhu/commit/75fff45ff1e221540361f1bfd38adf47582ed04b))
* introduce SetupContext to eliminate GetRegisteredSingleton hacks ([e7b3f83](https://github.com/st0o0/puhu/commit/e7b3f83c9bcc890eabf80413385ad9e180dc60c7))
* make ESC a global quit keybinding via GlobalKeyHandler ([4ff96b7](https://github.com/st0o0/puhu/commit/4ff96b73da4a27435ea28ac24ffeae415caa7a0c))
* make Servus.Plugin a fat SDK with Akka, R3, and Termina dependencies ([ee27047](https://github.com/st0o0/puhu/commit/ee27047057e1902bd5c49169d342831193a045a4))
* migrate MarketplacePlugin and SettingsPlugin to simplified builder API ([4466a36](https://github.com/st0o0/puhu/commit/4466a36325bfece53fe06f8468376de437e168da))
* move UI nodes to Plugin SDK, use AppShell.Wrap in MarketplacePage ([0e59e01](https://github.com/st0o0/puhu/commit/0e59e01058e03549e510f86419b8fd2d0c57f5ec))
* Organize project files into src directory ([5ab31ab](https://github.com/st0o0/puhu/commit/5ab31abf8f722cbcf9cb22059652dc5eb96acc19))
* **plugin:** Rename Puhu.Plugin namespaces ([4939842](https://github.com/st0o0/puhu/commit/4939842a6c9aa8f5bc303036a6904c4a73ff7883))
* **project:** Rename TUI projects to Puhu ([d6bf661](https://github.com/st0o0/puhu/commit/d6bf661d23e8215b2a5b6b422b6a7d672f2c5515))
* rename Servus.Plugin.Sdk to Servus.Plugin (directories and project files) ([fda2bda](https://github.com/st0o0/puhu/commit/fda2bdac2b1f70324ee863833639cc60bb6e9729))
* replace GlobalKeyHandler with page-level keybindings ([523e58f](https://github.com/st0o0/puhu/commit/523e58fa688c0bbe946a7721b108b0eb1b0c928a))
* simplify plugin builder to 4 methods, remove abstraction layers ([48372b9](https://github.com/st0o0/puhu/commit/48372b9b19d077bf7944edc18e8c7de8f06cda87))
* simplify plugin builder to 4 methods, remove abstraction layers ([1229eb3](https://github.com/st0o0/puhu/commit/1229eb3695821328149fe172fa454111add055c1))
* update all namespaces from Servus.Plugin.Sdk to Servus.Plugin ([91aaa39](https://github.com/st0o0/puhu/commit/91aaa3936d3dab2f26e7e823aef4b3bd34715d5d))

## Changelog
