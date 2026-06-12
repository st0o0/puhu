using R3;
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

    private readonly ProgressBarNode _progressBar = new ProgressBarNode()
        .WithGradient(Gradient.Create(Color.Cyan, Color.Blue))
        .WithLabel(" {0:P0}");

    public override ILayoutNode BuildLayout()
    {
        return Layouts.Vertical(
            new TextNode("").Fill(),
            new TextNode(Logo).AlignCenter(),
            new TextNode(""),
            Layouts.Horizontal(
                new TextNode("  "),
                _progressBar,
                new TextNode("  ")
            ),
            new TextNode(ViewModel.StatusText.Value).AlignCenter(),
            new TextNode("").Fill()
        );
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        ViewModel.Progress
            .Subscribe(v => _progressBar.WithValue(v))
            .DisposeWith(Subscriptions);
    }

    public override void Dispose()
    {
        _progressBar.Dispose();
        base.Dispose();
    }
}
