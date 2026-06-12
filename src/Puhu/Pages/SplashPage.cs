using Puhu.Themes;
using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Reactive;
using Termina.Terminal;

namespace Puhu.Pages;

public sealed class SplashPage : ReactivePage<SplashViewModel>
{
    private const string Logo = """
         ____        _
        |  _ \ _   _| |__  _   _
        | |_) | | | | '_ \| | | |
        |  __/| |_| | | | | |_| |
        |_|    \__,_|_| |_|\__,_|
        """;

    private readonly ProgressBarNode _progressBar;

    public SplashPage()
    {
        var theme = ThemeService.Instance.Current;
        _progressBar = new ProgressBarNode()
            .WithGradient(Gradient.Create(theme.Accent, theme.Selection))
            .WithLabel(" {0:P0}");
    }

    public override ILayoutNode BuildLayout()
    {
        var theme = ThemeService.Instance.Current;

        return Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode(Logo).WithForeground(theme.Accent).AlignCenter(),
            new TextNode(""),
            Layouts.Horizontal(
                new TextNode("  "),
                _progressBar,
                new TextNode("  ")
            ),
            new TextNode(ViewModel.StatusText.Value).WithForeground(theme.TextDim).AlignCenter(),
            new TextNode("").Fill(),
            new TextNode("Press ESC to quit").WithForeground(theme.TextDim).AlignCenter()
        );
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        ViewModel.Progress
            .Subscribe(v => _progressBar.WithValue(v))
            .DisposeWith(Subscriptions);

        ViewModel.Input.OfType<IInputEvent, KeyPressed>()
            .Where(k => k.KeyInfo.Key == ConsoleKey.Escape)
            .Subscribe(_ => ViewModel.RequestShutdown())
            .DisposeWith(Subscriptions);
    }

    public override void Dispose()
    {
        _progressBar.Dispose();
        base.Dispose();
    }
}
