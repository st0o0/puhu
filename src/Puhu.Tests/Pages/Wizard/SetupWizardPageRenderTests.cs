using System.Reflection;
using Puhu.Plugin;
using Puhu.Settings.Pages;
using Puhu.Tests.Fakes;
using Puhu.Tests.Helpers;
using R3;
using Termina.Layout;
using Termina.Reactive;
using Termina.Rendering;

namespace Puhu.Tests.Pages.Wizard;

public sealed class SetupWizardPageRenderTests
{
    private const int Width = 60;
    private const int Height = 16;

    [Fact]
    public void RendersShell_BreadcrumbAndStatusBar()
    {
        var (page, _) = CreateBoundPage();

        var view = RenderLayout(page.BuildLayout());

        Assert.Contains("welcome", view);   // breadcrumb
        Assert.Contains("1/6", view);        // status bar
        Assert.Contains("Puhu Setup", view); // panel title
        Assert.DoesNotContain("Step 1 of", view); // no legacy WizardNode block-bar progress
    }

    [Fact]
    public void WelcomeStep_DescribesPuhu()
    {
        var (page, _) = CreateBoundPage();

        var view = RenderLayout(page.BuildLayout());

        Assert.Contains("terminal dashboard", view);
    }

    private static (SetupWizardPage Page, SetupWizardViewModel Vm) CreateBoundPage()
    {
        var theme = new FakeThemeService(["alpha", "beta"]);
        var refresh = new FakeRefreshController();
        var tabs = new FakeTabOrderService(("System", "/system"));
        var store = new FakeSettingsStore();

        var page = new SetupWizardPage(theme);
        var vm = new SetupWizardViewModel(store, theme, refresh, tabs);

        var bind = typeof(ReactivePage<SetupWizardViewModel>)
            .GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ReactivePage<T>.Bind not found.");
        bind.Invoke(page, [vm]);

        return (page, vm);
    }

    private static string RenderLayout(ILayoutNode layout) => Tui.Render(layout, Width, Height).Snapshot();
}
