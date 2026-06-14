using Puhu.Plugin;
using R3;
using Termina.Layout;
using Termina.Reactive;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Pages;

public sealed class SplashPage : ReactivePage<SplashViewModel>
{
    private readonly ProgressBarNode _progressBar;
    private readonly ThemeDefinition _theme;

    public SplashPage(IThemeService themeService)
    {
        _theme = themeService.Current;
        _progressBar = new ProgressBarNode()
            .WithGradient(Gradient.Create(_theme.Accent, _theme.Selection))
            .WithLabel(" {0:P0}");
    }

    public override ILayoutNode BuildLayout()
    {
        var content = Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode(PuhuBranding.Logo).WithForeground(_theme.Accent).AlignCenter(),
            new TextNode(""),
            Layouts.Horizontal(
                new TextNode("  "),
                _progressBar,
                new TextNode("  ")
            ),
            new TextNode(ViewModel.StatusText.Value).WithForeground(_theme.TextDim).AlignCenter(),
            new TextNode("").Fill(),
            new TextNode("ESC Quit").WithForeground(_theme.TextDim).AlignCenter()
        );

        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(_theme.Border)
            .WithContent(content);
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        KeyBindings.Register(ConsoleKey.Escape, () => ViewModel.RequestShutdown());

        ViewModel.Progress
            .Subscribe(v => _progressBar.WithValue(v))
            .DisposeWith(Subscriptions);
    }

}
