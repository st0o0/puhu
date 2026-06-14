using System.Reflection;
using Puhu.Plugin;
using Puhu.Settings.Pages;
using R3;
using Termina.Layout;
using Termina.Reactive;
using Termina.Rendering;

namespace Puhu.Tests;

public sealed class SetupWizardPageRenderTests
{
    private const int Width = 60;
    private const int Height = 16;

    [Fact]
    public void RendersWelcomeStepWithProgress()
    {
        var (page, _) = CreateBoundPage();

        var view = RenderLayout(page.BuildLayout());

        Assert.Contains("Step 1 of", view);
    }

    private static (SetupWizardPage Page, SetupWizardViewModel Vm) CreateBoundPage()
    {
        var theme = new FakeThemeService(["alpha", "beta"]);
        var refresh = new FakeRefreshController();
        var tabs = new FakeTabOrderService(("System", "/system"));
        var store = new FakeStore();

        var page = new SetupWizardPage(theme);
        var vm = new SetupWizardViewModel(store, theme, refresh, tabs);

        var bind = typeof(ReactivePage<SetupWizardViewModel>)
            .GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ReactivePage<T>.Bind not found.");
        bind.Invoke(page, [vm]);

        return (page, vm);
    }

    private static string RenderLayout(ILayoutNode layout)
    {
        var ctx = new RenderTestContext(Width, Height);
        layout.Measure(new Size(Width, Height));
        layout.Render(ctx, new Rect(0, 0, Width, Height));
        var rows = new string[Height];
        for (var y = 0; y < Height; y++)
        {
            rows[y] = ctx.Row(y);
        }
        return string.Join('\n', rows);
    }

    private sealed class FakeStore : ISettingsStore
    {
        private readonly Dictionary<string, object?> _data = new();
        public T? Get<T>(string key) => _data.TryGetValue(key, out var v) ? (T?)v : default;
        public void Set<T>(string key, T value) => _data[key] = value;
        public Observable<T> Observe<T>(string key) => Observable.Empty<T>();
        public void Remove(string key) => _data.Remove(key);
    }

    private sealed class FakeThemeService(IReadOnlyList<string> themes) : IThemeService
    {
        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName => null;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => themes.ToList();
        public bool ApplyByName(string name) => true;
        public void SaveCurrent() { }
    }
}
